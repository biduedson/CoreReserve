namespace CoreReserve.Domain.Entities.UserAggregate.Events
{
    public class UserDeletedEvent(
        Guid id,
            string name,
            EGender gender,
            string email,
            DateTime createdAt
    ) : UserBaseEvent(id, name, gender, email, createdAt);
}