using BCrypt.Net;
using CoreReserve.Core.SharedKernel;

namespace CoreReserve.Infrastructure.Data.Services
{
    public class HashService : IHashService
    {
        private readonly int _workFactor;

        public HashService(int workFactor = 12)
        {
            _workFactor = workFactor;
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, _workFactor);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}