using MissionLog.Core.Enums;

namespace MissionLog.Core.Entities;

public class WorkOrder
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Draft;
    public Priority Priority { get; set; } = Priority.Medium;
    public string System { get; set; } = string.Empty; // e.g. "Avionics", "Propulsion", "Life Support"

    public int CreatedByUserId { get; set; }
    public User? CreatedBy { get; set; }

    public int? AssignedToUserId { get; set; }
    public User? AssignedTo { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }

    public ICollection<ApprovalAction> ApprovalActions { get; set; } = new List<ApprovalAction>();
    public ICollection<WorkOrderComment> Comments { get; set; } = new List<WorkOrderComment>();
}
