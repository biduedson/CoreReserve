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

            if (string.IsNullOrWhiteSpace(newPassword))
                return Result<Password>.Error("Password cannot be empty.");

            if (!RegexPatterns.HasUppercase.IsMatch(newPassword))
                return Result<Password>.Error("Password must contain at least one capital letter.");

            if (!RegexPatterns.HasDigit.IsMatch(newPassword))
                return Result<Password>.Error("Password must contain at least one number.");

            if (!RegexPatterns.HasSpecialCharacter.IsMatch(newPassword))
                return Result<Password>.Error("Password must contain at least one special character.");

            return Result<Password>.Success(new Password(newPassword));
        }
        public override string ToString() => NewPassword;
    }
}