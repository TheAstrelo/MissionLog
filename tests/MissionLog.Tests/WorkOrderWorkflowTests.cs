using MissionLog.Core.Entities;
using MissionLog.Core.Enums;

namespace MissionLog.Tests;

public class WorkOrderWorkflowTests
{
    [Fact]
    public void NewWorkOrder_HasDraftStatus()
    {
        var wo = new WorkOrder { Title = "Test WO", Description = "Fix engine", System = "Propulsion" };
        Assert.Equal(WorkOrderStatus.Draft, wo.Status);
    }

    [Fact]
    public void WorkOrder_StatusTransition_DraftToSubmitted()
    {
        var wo = new WorkOrder { Status = WorkOrderStatus.Draft };
        wo.Status = WorkOrderStatus.Submitted;
        Assert.Equal(WorkOrderStatus.Submitted, wo.Status);
    }

    [Fact]
    public void WorkOrder_StatusTransition_SubmittedToApproved()
    {
        var wo = new WorkOrder { Status = WorkOrderStatus.Submitted };
        wo.Status = WorkOrderStatus.Approved;
        Assert.Equal(WorkOrderStatus.Approved, wo.Status);
    }

    [Fact]
    public void WorkOrder_Completion_SetsCompletedAt()
    {
        var wo = new WorkOrder { Status = WorkOrderStatus.InProgress };
        wo.Status = WorkOrderStatus.Completed;
        wo.CompletedAt = DateTime.UtcNow;
        Assert.NotNull(wo.CompletedAt);
    }

    [Fact]
    public void WorkOrder_Priority_DefaultIsMedium()
    {
        var wo = new WorkOrder();
        Assert.Equal(Priority.Medium, wo.Priority);
    }

    [Fact]
    public void ApprovalAction_AttachesToWorkOrder()
    {
        var wo = new WorkOrder { Id = 1, Title = "WO-001" };
        var action = new ApprovalAction { WorkOrderId = 1, UserId = 2, Action = "Approved" };
        wo.ApprovalActions.Add(action);

        Assert.Single(wo.ApprovalActions);
        Assert.Equal("Approved", wo.ApprovalActions.First().Action);
    }

    [Theory]
    [InlineData(Priority.Low)]
    [InlineData(Priority.Medium)]
    [InlineData(Priority.High)]
    [InlineData(Priority.Critical)]
    public void WorkOrder_AcceptsAllPriorityLevels(Priority priority)
    {
        var wo = new WorkOrder { Priority = priority };
        Assert.Equal(priority, wo.Priority);
    }
}
