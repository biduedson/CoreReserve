using Ardalis.Result;
using CoreReserve.Domain.Entities.UserAggregate;
using CoreReserve.Domain.ValueObjects;

namespace CoreReserve.Domain.Factories
{
    public static class UserFactory
    {
        public static Result<User> Create(
             string name,
            EGender gender,
            string email,
            string password,
            DateTime createdAt
        )
        {
            var emailResult = Email.Create(email);
            return !emailResult.IsSuccess
                   ? Result<User>.Error(new ErrorList(emailResult.Errors.ToArray()))
                   : Result<User>.Success(new User(name, gender, emailResult.Value, password, createdAt));
        }

        public static User Create(string name, EGender gender, Email email, string password, DateTime createdAt)
           => new(name, gender, email, password, createdAt);
    }
}