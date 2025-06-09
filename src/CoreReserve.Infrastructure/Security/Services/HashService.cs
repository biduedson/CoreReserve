using BCrypt.Net;
using CoreReserve.Core.AppSettings;
using CoreReserve.Core.SharedKernel;
using Microsoft.Extensions.Options;

namespace CoreReserve.Infrastructure.Security.Services
{
    internal sealed class HashService(IOptions<SecuriTyOptions> options) : IHashService
    {
        public string HashPassword(string password)
        {
            var workFactor = options.Value.Bcrypt.WorkFactor;
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}