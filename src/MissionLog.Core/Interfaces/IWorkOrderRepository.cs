using MissionLog.Core.Entities;
using MissionLog.Core.Enums;

namespace MissionLog.Core.Interfaces;

public interface IWorkOrderRepository
{
    Task<IEnumerable<WorkOrder>> GetAllAsync();
    Task<IEnumerable<WorkOrder>> GetByStatusAsync(WorkOrderStatus status);
    Task<IEnumerable<WorkOrder>> GetByUserAsync(int userId);
    Task<WorkOrder?> GetByIdAsync(int id);
    Task<WorkOrder> CreateAsync(WorkOrder workOrder);
    Task<WorkOrder> UpdateAsync(WorkOrder workOrder);
    Task DeleteAsync(int id);
}
