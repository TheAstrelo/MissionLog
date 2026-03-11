using Microsoft.EntityFrameworkCore;
using MissionLog.Core.Entities;
using MissionLog.Core.Enums;
using MissionLog.Core.Interfaces;
using MissionLog.Infrastructure.Data;

namespace MissionLog.Infrastructure.Repositories;

public class WorkOrderRepository : IWorkOrderRepository
{
    private readonly AppDbContext _context;

    public WorkOrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<WorkOrder>> GetAllAsync() =>
        await _context.WorkOrders
            .Include(w => w.CreatedBy)
            .Include(w => w.AssignedTo)
            .Include(w => w.ApprovalActions).ThenInclude(a => a.User)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<WorkOrder>> GetByStatusAsync(WorkOrderStatus status) =>
        await _context.WorkOrders
            .Include(w => w.CreatedBy)
            .Include(w => w.AssignedTo)
            .Where(w => w.Status == status)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<WorkOrder>> GetByUserAsync(int userId) =>
        await _context.WorkOrders
            .Include(w => w.CreatedBy)
            .Include(w => w.AssignedTo)
            .Where(w => w.CreatedByUserId == userId || w.AssignedToUserId == userId)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();

    public async Task<WorkOrder?> GetByIdAsync(int id) =>
        await _context.WorkOrders
            .Include(w => w.CreatedBy)
            .Include(w => w.AssignedTo)
            .Include(w => w.ApprovalActions).ThenInclude(a => a.User)
            .Include(w => w.Comments).ThenInclude(c => c.User)
            .FirstOrDefaultAsync(w => w.Id == id);

    public async Task<WorkOrder> CreateAsync(WorkOrder workOrder)
    {
        _context.WorkOrders.Add(workOrder);
        await _context.SaveChangesAsync();
        return workOrder;
    }

    public async Task<WorkOrder> UpdateAsync(WorkOrder workOrder)
    {
        workOrder.UpdatedAt = DateTime.UtcNow;
        _context.WorkOrders.Update(workOrder);
        await _context.SaveChangesAsync();
        return workOrder;
    }

    public async Task DeleteAsync(int id)
    {
        var wo = await _context.WorkOrders.FindAsync(id);
        if (wo != null)
        {
            _context.WorkOrders.Remove(wo);
            await _context.SaveChangesAsync();
        }
    }
}
