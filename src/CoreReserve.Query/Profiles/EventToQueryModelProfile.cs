using AutoMapper;
using CoreReserve.Domain.Entities.UserAggregate.Events;
using CoreReserve.Query.QueriesModel;

namespace CoreReserve.Query.Profiles
{
    /// <summary>
    /// Perfil de mapeamento para converter eventos de usuários em modelos de consulta.
    /// Permite que os eventos sejam transformados em objetos de consulta para uso em Query Handlers.
    /// </summary>
    public class EventToQueryModelProfile : Profile
    {
        /// <summary>
        /// Configura o mapeamento dos eventos para o modelo de consulta do usuário.
        /// </summary>
        public EventToQueryModelProfile()
        {
            CreateMap<UserCreatedEvent, UserQueryModel>(MemberList.Destination)
                .ConstructUsing(@event => CreateUserQueryModel(@event));

            CreateMap<UserUpdatedEvent, UserQueryModel>(MemberList.Destination)
                .ConstructUsing(@event => CreateUserQueryModel(@event));

            CreateMap<UserDeletedEvent, UserQueryModel>(MemberList.Destination)
                .ConstructUsing(@event => CreateUserQueryModel(@event));
        }

        /// <summary>
        /// Obtém o nome do perfil de mapeamento.
        /// </summary>
        public override string ProfileName => nameof(EventToQueryModelProfile);

        /// <summary>
        /// Método auxiliar para criar um modelo de consulta a partir de um evento.
        /// </summary>
        private static UserQueryModel CreateUserQueryModel<TEvent>(TEvent @event) where TEvent : UserBaseEvent =>
            new(@event.Id, @event.Name, @event.Gender.ToString(), @event.Email, @event.CreatedAt);
    }
}

// -----------------------------------------
// 🔹 EXPLICAÇÃO DETALHADA 🔹
// -----------------------------------------

/*
✅ Classe EventToQueryModelProfile → Define o perfil de mapeamento do AutoMapper para transformar eventos em modelos de consulta.
✅ Herança de Profile → Permite que a classe se integre automaticamente ao AutoMapper.
✅ Métodos CreateMap() → Configura a conversão de eventos (`UserCreatedEvent`, `UserUpdatedEvent`, `UserDeletedEvent`) para `UserQueryModel`.
✅ Uso de ConstructUsing() → Permite customizar a criação do modelo de consulta com base nos eventos recebidos.
✅ Método CreateUserQueryModel() → Garante que todos os eventos geram um `UserQueryModel` consistente.
✅ Arquitetura baseada em CQRS → Mantém separação entre comandos (eventos) e consultas (query models).
✅ Essa abordagem melhora a rastreabilidade e a eficiência na manipulação de dados em tempo real.
*/
