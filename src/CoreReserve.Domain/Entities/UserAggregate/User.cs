using CoreReserve.Core.SharedKernel;
using CoreReserve.Domain.Entities.UserAggregate.Events;
using CoreReserve.Domain.ValueObjects;

namespace CoreReserve.Domain.Entities.UserAggregate
{
    /// <summary>
    /// Representa um usuário do sistema.
    /// Aggregate Root responsável por gerenciar informações e comportamentos relacionados ao usuário.
    /// </summary>
    public class User : BaseEntity, IAggregateRoot
    {
        #region Fields

        private bool _isDeleted;

        #endregion

        #region Constructors

        /// <summary>
        /// Cria uma nova instância de usuário.
        /// </summary>
        /// <param name="name">Nome do usuário.</param>
        /// <param name="gender">Gênero do usuário.</param>
        /// <param name="email">Email do usuário.</param>
        /// <param name="password">Senha do usuário.</param>
        public User(string name, EGender gender, Email email, Password password)
        {
            Name = name;
            Gender = gender;
            Email = email;
            Password = password;

            AddDomainEvent(new UserCreatedEvent(Id, name, gender, email.Address, CreatedAt));
        }

        /// <summary>
        /// Construtor privado para uso do Entity Framework Core.
        /// </summary>
        private User() { }

        #endregion

        #region Properties

        /// <summary>
        /// Nome do usuário.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gênero do usuário.
        /// </summary>
        public EGender Gender { get; }

        /// <summary>
        /// Email do usuário.
        /// </summary>
        public Email Email { get; private set; }

        /// <summary>
        /// Senha do usuário.
        /// </summary>
        public Password Password { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Altera o email do usuário.
        /// </summary>
        /// <param name="newEmail">Novo email a ser definido.</param>
        public void ChangeEmail(Email newEmail)
        {
            if (Email.Equals(newEmail))
                return;

            Email = newEmail;
        }

        /// <summary>
        /// Marca o usuário como deletado e dispara o evento correspondente.
        /// </summary>
        public void Delete()
        {
            if (_isDeleted)
                return;

            _isDeleted = true;
            AddDomainEvent(new UserDeletedEvent(Id, Name, Gender, Email.Address, CreatedAt));
        }

        #endregion
    }
}