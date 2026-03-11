namespace MissionLog.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IWorkOrderRepository WorkOrders { get; }
    IUserRepository Users { get; }
    Task<int> SaveChangesAsync();
}
