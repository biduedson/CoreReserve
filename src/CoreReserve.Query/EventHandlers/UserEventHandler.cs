using AutoMapper;
using CoreReserve.Core.Extensions;
using CoreReserve.Core.SharedKernel;
using CoreReserve.Domain.Entities.UserAggregate.Events;
using CoreReserve.Query.Abstractions;
using CoreReserve.Query.Application.User.Queries;
using CoreReserve.Query.QueriesModel;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreReserve.Query.EventHandlers
{
    /// <summary>
    /// Manipulador de eventos do usuário.
    /// Responsável por processar eventos relacionados à criação, atualização e exclusão de usuários.
    /// Atua sincronizando o banco de dados e limpando o cache para garantir integridade dos dados.
    /// </summary>
    public class UserEventHandler(
        IMapper mapper,
        ISynchronizeDb synchronizeDb,
        ICacheService cacheService,
        ILogger<UserEventHandler> logger) :
        INotificationHandler<UserCreatedEvent>,
        INotificationHandler<UserUpdatedEvent>,
        INotificationHandler<UserDeletedEvent>
    {
        /// <summary>
        /// Manipula o evento de criação de usuário.
        /// Registra o evento, insere os dados no banco de consultas e limpa o cache.
        /// </summary>
        public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
        {
            LogEvent(notification);

            var userQueryModel = mapper.Map<UserQueryModel>(notification);
            await synchronizeDb.UpsertAsync(userQueryModel, filter => filter.Id == userQueryModel.Id);
            await ClearCacheAsync(notification);
        }

        /// <summary>
        /// Manipula o evento de exclusão de usuário.
        /// Registra o evento, remove os dados do banco de consultas e limpa o cache.
        /// </summary>
        public async Task Handle(UserDeletedEvent notification, CancellationToken cancellationToken)
        {
            LogEvent(notification);

            await synchronizeDb.DeleteAsync<UserQueryModel>(filter => filter.Email == notification.Email);
            await ClearCacheAsync(notification);
        }

        /// <summary>
        /// Manipula o evento de atualização de usuário.
        /// Registra o evento, atualiza os dados no banco de consultas e limpa o cache.
        /// </summary>
        public async Task Handle(UserUpdatedEvent notification, CancellationToken cancellationToken)
        {
            LogEvent(notification);

            var userQueryModel = mapper.Map<UserQueryModel>(notification);
            await synchronizeDb.UpsertAsync(userQueryModel, filter => filter.Id == userQueryModel.Id);
            await ClearCacheAsync(notification);
        }

        /// <summary>
        /// Limpa o cache relacionado ao usuário afetado pelo evento.
        /// Garante que consultas futuras obtenham dados atualizados.
        /// </summary>
        private async Task ClearCacheAsync(UserBaseEvent @event)
        {
            var cacheKeys = new[] { nameof(GetAllUserQuery), $"{nameof(GetUserByIdQuery)}_{@event.Id}" };
            await cacheService.RemoveAsync(cacheKeys);
        }

        /// <summary>
        /// Registra informações sobre o evento, incluindo nome e modelo dos dados.
        /// </summary>
        private void LogEvent<TEvent>(TEvent @event) where TEvent : UserBaseEvent =>
            logger.LogInformation("----- Evento disparado {EventName}, modelo: {EventModel}", typeof(TEvent).Name, @event.ToJson());
    }
}

// -----------------------------------------
// 🔹 EXPLICAÇÃO DETALHADA 🔹
// -----------------------------------------

/*
✅ Classe UserEventHandler → Processa eventos de usuários, garantindo sincronização com banco de dados e cache.
✅ Implementação de INotificationHandler → Define como os eventos são tratados na arquitetura de mensageria.
✅ Uso de AutoMapper → Facilita a conversão dos eventos em modelos de consulta.
✅ Interação com ISynchronizeDb → Permite inserção, atualização e remoção de usuários no banco de consultas.
✅ Uso de ICacheService → Remove registros armazenados para garantir consistência nos dados consultados.
✅ Método LogEvent() → Registra os eventos processados, facilitando auditoria e rastreabilidade.
✅ Método ClearCacheAsync() → Remove os cacheKeys correspondentes ao usuário afetado pelo evento.
✅ Arquitetura baseada em CQRS → Mantém separação entre comandos e consultas, melhorando escalabilidade.
✅ Essa estrutura torna a gestão de eventos robusta, garantindo que os dados da aplicação sejam sempre atualizados corretamente.
*/
