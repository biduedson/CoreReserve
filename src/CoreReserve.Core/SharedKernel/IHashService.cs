namespace CoreReserve.Core.SharedKernel
{
    public interface IHashService
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
    }
}