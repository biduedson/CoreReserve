using Ardalis.Result;
using CoreReserve.Core;

namespace CoreReserve.Domain.ValueObjects
{
    public sealed record Email
    {
        private Email(string address) => Address = address.ToLowerInvariant().Trim();

        public Email() { }
        public string Address { get; }

        public static Result<Email> Create(string emailAddress)
        {
            if (string.IsNullOrWhiteSpace(emailAddress))
                return Result<Email>.Error("The e-mail address must be provided.");

            return !RegexPatterns.EmailIsValid.IsMatch(emailAddress)
                ? Result<Email>.Error("The e-mail address is invalid.")
                : Result<Email>.Success(new Email(emailAddress));
        }
        public override string ToString() => Address;
    }
}