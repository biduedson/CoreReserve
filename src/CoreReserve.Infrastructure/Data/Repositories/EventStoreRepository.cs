using CoreReserve.Core.SharedKernel;
using CoreReserve.Infrastructure.Data.Context;

namespace CoreReserve.Infrastructure.Data.Repositories
{
    /// <summary>
    /// Repositório responsável por armazenar eventos no Event Store.
    /// Implementa operações de persistência assíncrona e gerenciamento de recursos.
    /// </summary>
    internal sealed class EventStoreRepository(EventStoreDbContext dbContext) : IEventStoreRepository
    {
        /// <summary>
        /// Salva uma coleção de eventos no banco de dados de forma assíncrona.
        /// </summary>
        /// <param name="eventStores">Os eventos a serem armazenados.</param>
        public async Task StoreAsync(IEnumerable<EventStore> eventStores)
        {
            await dbContext.EventStores.AddRangeAsync(eventStores);
            await dbContext.SaveChangesAsync();
        }

        #region IDisposable

        // Para detectar chamadas redundantes.
        private bool _disposed;

        /// <summary>
        /// Implementação do padrão Dispose para liberar recursos corretamente.
        /// </summary>
        ~EventStoreRepository() => Dispose(false);

        /// <summary>
        /// Implementação pública do padrão Dispose, chamável por consumidores.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Implementação protegida do padrão Dispose.
        /// </summary>
        /// <param name="disposing">Define se a liberação de recursos está ocorrendo explicitamente.</param>
        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            // Libera recursos gerenciados
            if (disposing)
                dbContext.Dispose();

            _disposed = true;
        }

        #endregion
    }

    // -----------------------------------------
    // 🔹 EXPLICAÇÃO DO CÓDIGO 🔹
    // -----------------------------------------
    /*
    ✅ Classe EventStoreRepository → Implementa um repositório responsável por armazenar eventos no Event Store. 
    ✅ Método StoreAsync() → Permite salvar eventos de forma assíncrona, garantindo eficiência na persistência de dados. 
    ✅ Uso de IDisposable → Garante que recursos como conexões do banco sejam corretamente liberados quando não necessários. 
    ✅ Implementação de Dispose() → Evita vazamento de memória ao liberar objetos gerenciados no encerramento da instância. 
    ✅ Essa abordagem melhora a escalabilidade e mantém a consistência do Event Store, garantindo auditoria e rastreabilidade dos eventos. 
    */

}