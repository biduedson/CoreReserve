using BCrypt.Net;
using CoreReserve.Core.AppSettings;
using CoreReserve.Core.SharedKernel;
using Microsoft.Extensions.Options;

namespace CoreReserve.Infrastructure.Security.Services
{
    public class HashService : IHashService
    {

        private readonly IOptions<SecuriTyOptions> _options;

        public HashService(IOptions<SecuriTyOptions> options)
        {
            _options = options;
        }

        public string HashPassword(string password)
        {
            var workFactor = _options.Value.Bcrypt.WorkFactor;
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}