namespace CoreReserve.Core.SharedKernel
{
    public interface IEventStoreRepository : IDisposable
    {
        Task StoreAsync(IEnumerable<EventStore> eventStores);
    }
}