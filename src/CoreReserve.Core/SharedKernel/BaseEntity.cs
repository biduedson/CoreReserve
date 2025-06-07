namespace CoreReserve.Core.SharedKernel
{
    /// <summary>
    /// Classe base para todas as entidades do domínio.
    /// Fornece funcionalidades comuns como identificação única e gerenciamento de eventos de domínio.
    /// </summary>
    public abstract class BaseEntity : IEntity<Guid>
    {
        #region Fields

        private readonly List<BaseEvent> _domainEvents = [];

        #endregion

        #region Constructors

        /// <summary>
        /// Construtor padrão que gera um novo ID e define a data de criação.
        /// </summary>
        protected BaseEntity()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.Now;
        }

        /// <summary>
        /// Construtor que permite definir um ID específico.
        /// </summary>
        /// <param name="id">Identificador único da entidade.</param>
        protected BaseEntity(Guid id) => Id = id;

        /// <summary>
        /// Construtor que permite definir uma data de criação específica.
        /// </summary>
        /// <param name="createdAt">Data de criação da entidade.</param>
        protected BaseEntity(DateTime createdAt) => CreatedAt = createdAt;

        #endregion

        #region Properties

        /// <summary>
        /// Identificador único da entidade.
        /// </summary>
        public Guid Id { get; private init; }

        /// <summary>
        /// Data e hora de criação da entidade.
        /// </summary>
        public DateTime CreatedAt { get; private init; }

        /// <summary>
        /// Coleção somente leitura dos eventos de domínio associados à entidade.
        /// </summary>
        public IEnumerable<BaseEvent> DomainEvents => _domainEvents.AsReadOnly();

        #endregion

        #region Methods

        /// <summary>
        /// Adiciona um evento de domínio à entidade.
        /// </summary>
        /// <param name="domainEvent">Evento de domínio a ser adicionado.</param>
        protected void AddDomainEvent(BaseEvent domainEvent) => _domainEvents.Add(domainEvent);

        /// <summary>
        /// Remove todos os eventos de domínio da entidade.
        /// </summary>
        public void ClearDomainEvents() => _domainEvents.Clear();

        #endregion
    }
}