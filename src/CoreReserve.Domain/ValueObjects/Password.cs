using System.Text.RegularExpressions;
using Ardalis.Result;
using CoreReserve.Core;

namespace CoreReserve.Domain.ValueObjects
{

    public sealed record Password
    {
        private Password(string password) => NewPassword = password;

        public string NewPassword { get; }

        public Password() { } //Efcore

        public static Result<Password> Create(string newPassword)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(newPassword))
                errors.AddRange("Password cannot be empty.");

            if (!RegexPatterns.HasUppercase.IsMatch(newPassword))
                errors.AddRange("Password must contain at least one capital letter.");

            if (!RegexPatterns.HasDigit.IsMatch(newPassword))
                errors.AddRange("Password must contain at least one number.");

            if (!RegexPatterns.HasSpecialCharacter.IsMatch(newPassword))
                errors.AddRange("Password must contain at least one special character.");

            return errors.Any() ? Result<Password>.Error(new ErrorList(errors.ToArray()))
                               : Result<Password>.Success(new Password(newPassword));
        }
        public override string ToString() => NewPassword;
    }
}