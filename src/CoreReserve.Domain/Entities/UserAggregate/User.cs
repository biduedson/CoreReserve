using CoreReserve.Core.SharedKernel;
using CoreReserve.Domain.Entities.UserAggregate.Events;
using CoreReserve.Domain.ValueObjects;

namespace CoreReserve.Domain.Entities.UserAggregate
{
    public class User : BaseEntity, IAggregateRoot
    {
        private bool _isDeleted;
        public string Name { get; }
        public EGender Gender { get; }
        public Email Email { get; private set; }
        public Password Password { get; private set; }
        public DateTime CreatedAt { get; }

        public User(string name, EGender gender, Email email, Password password, DateTime createdAt)
        {
            Name = name;
            Gender = gender;
            Email = email;
            Password = password;
            CreatedAt = createdAt;

            AddDomainEvent(new UserCreatedEvent(Id, name, gender, email.Address, createdAt));
        }

        private User() { }

        public void ChangeEmail(Email newEmail)
        {
            if (Email.Equals(newEmail))
                return;
            Email = newEmail;
        }

        public void Delete()
        {

            if (_isDeleted) return;

            _isDeleted = true;
            AddDomainEvent(new UserDeletedEvent(Id, Name, Gender, Email.Address, CreatedAt));
        }
    }
}