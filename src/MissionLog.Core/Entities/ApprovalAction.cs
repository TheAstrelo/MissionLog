namespace MissionLog.Core.Entities;

public class ApprovalAction
{
    public int Id { get; set; }
    public int WorkOrderId { get; set; }
    public WorkOrder? WorkOrder { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public string Action { get; set; } = string.Empty; // "Submitted" | "Approved" | "Rejected" | "Reassigned"
    public string? Notes { get; set; }
    public DateTime ActionDate { get; set; } = DateTime.UtcNow;
}
