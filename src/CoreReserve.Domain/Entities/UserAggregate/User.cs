using CoreReserve.Core.SharedKernel;
using CoreReserve.Domain.Entities.UserAggregate.Events;
using CoreReserve.Domain.ValueObjects;

namespace CoreReserve.Domain.Entities.UserAggregate
{
    public class User : BaseEntity
    {
        private bool _isDeleted;

        public string Name { get; }
        public EGender Gender { get; }
        public Email Email { get; private set; }
        public string Password { get; }
        public bool IsActive { get; } = true;
        public DateTime CreatedAt { get; }

        public User(string name, EGender gender, Email email, DateTime createdAt)
        {
            Name = name;
            Gender = gender;
            Email = email;
            CreatedAt = createdAt;

            AddDomainEvent(new UserCreatedEvent(Id, name, gender, email.Address, createdAt));
        }

        public User() { }

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
        }
    }
}