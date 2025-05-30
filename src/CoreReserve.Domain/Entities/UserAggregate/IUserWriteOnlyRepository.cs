using CoreReserve.Core.SharedKernel;
using CoreReserve.Domain.ValueObjects;

namespace CoreReserve.Domain.Entities.UserAggregate
{
    public interface IUserWriteOnlyRepository : IWriteOnlyRepository<User, Guid>
    {
        Task<bool> ExistsByEmailAsync(Email email);
        Task<bool> ExistsByEmailAsync(Email email, Guid currentId);
    }
}