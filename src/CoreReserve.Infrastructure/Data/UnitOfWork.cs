using System.Data;
using CoreReserve.Core.Extensions;
using CoreReserve.Core.SharedKernel;
using CoreReserve.Infrastructure.Data.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreReserve.Infrastructure.Data
{
    /// <summary>
    /// Implementação do padrão Unit of Work, responsável pelo gerenciamento de transações e persistência no banco de dados.
    /// </summary>
    internal sealed class UnitOfWork(
        WriteDbContext writeDbContext,
        IEventStoreRepository eventStoreRepository,
        IMediator mediator,
        ILogger<UnitOfWork> logger) : IUnitOfWork
    {
        /// <summary>
        /// Salva as alterações no banco de dados de forma assíncrona.
        /// Implementa estratégia de execução para garantir resiliência e evitar falhas na conexão.
        /// </summary>
        public async Task SaveChangesAsync()
        {
            // Criando a estratégia de execução (Resiliência de conexão e tentativas de banco de dados).
            var strategy = writeDbContext.Database.CreateExecutionStrategy();

            // Executando a estratégia.
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await writeDbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

                logger.LogInformation("----- Iniciando transação: '{TransactionId}'", transaction.TransactionId);

                try
                {
                    // Obtendo eventos de domínio e eventos armazenáveis das entidades rastreadas no contexto do EF Core.
                    var (domainEvents, eventStores) = BeforeSaveChanges();

                    var rowsAffected = await writeDbContext.SaveChangesAsync();

                    logger.LogInformation("----- Commit da transação: '{TransactionId}'", transaction.TransactionId);

                    await transaction.CommitAsync();

                    // Disparando os eventos e armazenando os eventos no Event Store.
                    await AfterSaveChangesAsync(domainEvents, eventStores);

                    logger.LogInformation(
                        "----- Transação confirmada com sucesso: '{TransactionId}', Linhas afetadas: {RowsAffected}",
                        transaction.TransactionId,
                        rowsAffected);
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "Ocorreu uma exceção inesperada ao confirmar a transação: '{TransactionId}', mensagem: {Message}",
                        transaction.TransactionId,
                        ex.Message);

                    await transaction.RollbackAsync();

                    throw;
                }
            });
        }

        /// <summary>
        /// Executa lógica antes de salvar as alterações no banco de dados.
        /// </summary>
        /// <returns>Uma tupla contendo a lista de eventos de domínio e eventos do Event Store.</returns>
        private (IReadOnlyList<BaseEvent> domainEvents, IReadOnlyList<EventStore> eventStores) BeforeSaveChanges()
        {
            // Obtém todas as entidades de domínio com eventos pendentes
            var domainEntities = writeDbContext
                .ChangeTracker
                .Entries<BaseEntity>()
                .Where(entry => entry.Entity.DomainEvents.Any())
                .ToList();

            // Obtém todos os eventos de domínio das entidades
            var domainEvents = domainEntities
                .SelectMany(entry => entry.Entity.DomainEvents)
                .ToList();

            // Converte eventos de domínio em eventos armazenáveis
            var eventStores = domainEvents
                .ConvertAll(@event => new EventStore(@event.AggregateId, @event.GetGenericTypeName(), @event.ToJson()));

            // Limpa eventos de domínio das entidades
            domainEntities.ForEach(entry => entry.Entity.ClearDomainEvents());

            return (domainEvents.AsReadOnly(), eventStores.AsReadOnly());
        }

        /// <summary>
        /// Executa ações após salvar as alterações, como publicação de eventos de domínio e armazenamento no Event Store.
        /// </summary>
        /// <param name="domainEvents">Lista de eventos de domínio.</param>
        /// <param name="eventStores">Lista de eventos do Event Store.</param>
        /// <returns>Uma tarefa representando a operação assíncrona.</returns>
        private async Task AfterSaveChangesAsync(
            IReadOnlyList<BaseEvent> domainEvents,
            IReadOnlyList<EventStore> eventStores)
        {
            // Publica cada evento de domínio usando _mediator.
            if (domainEvents.Count > 0)
                await Task.WhenAll(domainEvents.Select(@event => mediator.Publish(@event)));

            // Armazena os eventos no Event Store usando _eventStoreRepository.
            if (eventStores.Count > 0)
                await eventStoreRepository.StoreAsync(eventStores);
        }

        #region IDisposable

        // Para detectar chamadas redundantes.
        private bool _disposed;

        // Implementação pública do padrão Dispose, chamável por consumidores.
        ~UnitOfWork() => Dispose(false);

        // Implementação pública do padrão Dispose, chamável por consumidores.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Implementação protegida do padrão Dispose.
        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            // Libera recursos gerenciados
            if (disposing)
            {
                writeDbContext.Dispose();
                eventStoreRepository.Dispose();
            }

            _disposed = true;
        }

        #endregion
    }

    // -----------------------------------------
    // 🔹 EXPLICAÇÃO DO CÓDIGO 🔹
    // -----------------------------------------
    /*
    ✅ Classe UnitOfWork → Implementa o padrão Unit of Work, garantindo controle de transações e persistência no banco de dados. 
    ✅ Estratégia de execução → Cria uma estratégia para garantir resiliência de conexão e tentativas de banco de dados. 
    ✅ Métodos BeforeSaveChanges() e AfterSaveChangesAsync() → Capturam eventos de domínio antes da gravação e os processam após o commit. 
    ✅ Implementação de IDisposable → Libera corretamente recursos ao encerrar a instância. 
    ✅ Uso de MediatR para publicação de eventos → Garante comunicação desacoplada entre domínios dentro da aplicação. 
    ✅ Uso de Event Store → Registra eventos para futura auditoria e rastreamento. 
    */
}