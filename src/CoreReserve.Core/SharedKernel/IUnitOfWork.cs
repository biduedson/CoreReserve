namespace CoreReserve.Core.SharedKernel
{
    public interface IUnitOfWork : IDisposable
    {
        Task SaveChangesAsync();
    }
}