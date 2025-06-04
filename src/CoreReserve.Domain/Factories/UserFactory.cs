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
            var passwordResult = Password.Create(password);
            var errors = new List<string>();

            if (!emailResult.IsSuccess)
                errors.AddRange(emailResult.Errors);

            if (!passwordResult.IsSuccess)
                errors.AddRange(passwordResult.Errors);

            return errors.Any() ? Result<User>.Error(new ErrorList(errors.ToArray())) :
            Result<User>.Success(new User(name, gender, emailResult.Value, passwordResult.Value, createdAt));

        }

        public static User Create(string name, EGender gender, Email email, Password password, DateTime createdAt)
           => new(name, gender, email, password, createdAt);
    }
}