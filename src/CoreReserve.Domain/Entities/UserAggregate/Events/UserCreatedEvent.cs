namespace CoreReserve.Domain.Entities.UserAggregate.Events
{
    public class UserCreatedEvent(
        Guid id,
            string name,
            EGender gender,
            string email,
            DateTime createdAt
    ) : UserBaseEvent(id, name, gender, email, createdAt);
}