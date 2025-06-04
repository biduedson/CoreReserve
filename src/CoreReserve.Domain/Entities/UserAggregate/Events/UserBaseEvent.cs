using CoreReserve.Core.SharedKernel;
using CoreReserve.Domain.ValueObjects;

namespace CoreReserve.Domain.Entities.UserAggregate.Events
{
    public abstract class UserBaseEvent : BaseEvent
    {
        protected UserBaseEvent(
            Guid id,
            string name,
            EGender gender,
            string email,
            bool isActive,
            DateTime createdAt)
        {
            Id = id;
            AggregateId = id;
            Name = name;
            Gender = gender;
            Email = email;
            IsActive = isActive;
            CreatedAt = createdAt;
        }
        public Guid Id { get; private init; }
        public string Name { get; private init; }
        public EGender Gender { get; private init; }
        public string Email { get; private init; }
        public bool IsActive { get; } = true;
        public DateTime CreatedAt { get; private init; }
    }
}