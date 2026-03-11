using MissionLog.Core.Interfaces;
using MissionLog.Infrastructure.Data;
using MissionLog.Infrastructure.Repositories;

namespace MissionLog.Infrastructure.Services;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public IWorkOrderRepository WorkOrders { get; }
    public IUserRepository Users { get; }

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        WorkOrders = new WorkOrderRepository(context);
        Users = new UserRepository(context);
    }

    public async Task<int> SaveChangesAsync() =>
        await _context.SaveChangesAsync();

    public void Dispose() => _context.Dispose();
}
