using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MissionLog.API.Hubs;
using MissionLog.Core.DTOs;
using MissionLog.Core.Entities;
using MissionLog.Core.Enums;
using MissionLog.Core.Interfaces;

namespace MissionLog.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkOrdersController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly IHubContext<WorkOrderHub> _hub;

    public WorkOrdersController(IUnitOfWork uow, IHubContext<WorkOrderHub> hub)
    {
        _uow = uow;
        _hub = hub;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string CurrentRole => User.FindFirstValue(ClaimTypes.Role)!;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkOrderDto>>> GetAll([FromQuery] WorkOrderStatus? status)
    {
        var workOrders = status.HasValue
            ? await _uow.WorkOrders.GetByStatusAsync(status.Value)
            : await _uow.WorkOrders.GetAllAsync();

        return Ok(workOrders.Select(MapToDto));
    }

    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<WorkOrderDto>>> GetMine()
    {
        var workOrders = await _uow.WorkOrders.GetByUserAsync(CurrentUserId);
        return Ok(workOrders.Select(MapToDto));
    }

    [HttpGet("summary")]
    public async Task<ActionResult<WorkOrderSummaryDto>> GetSummary()
    {
        var all = await _uow.WorkOrders.GetAllAsync();
        var list = all.ToList();
        return Ok(new WorkOrderSummaryDto(
            Total: list.Count,
            Draft: list.Count(w => w.Status == WorkOrderStatus.Draft),
            UnderReview: list.Count(w => w.Status == WorkOrderStatus.UnderReview),
            Approved: list.Count(w => w.Status == WorkOrderStatus.Approved),
            InProgress: list.Count(w => w.Status == WorkOrderStatus.InProgress),
            Completed: list.Count(w => w.Status == WorkOrderStatus.Completed),
            Rejected: list.Count(w => w.Status == WorkOrderStatus.Rejected)
        ));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WorkOrderDto>> GetById(int id)
    {
        var wo = await _uow.WorkOrders.GetByIdAsync(id);
        if (wo == null) return NotFound();
        return Ok(MapToDto(wo));
    }

    [HttpPost]
    public async Task<ActionResult<WorkOrderDto>> Create([FromBody] CreateWorkOrderDto dto)
    {
        var wo = new WorkOrder
        {
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            System = dto.System,
            DueDate = dto.DueDate,
            CreatedByUserId = CurrentUserId,
            Status = WorkOrderStatus.Draft
        };

        await _uow.WorkOrders.CreateAsync(wo);
        var created = await _uow.WorkOrders.GetByIdAsync(wo.Id);

        await _hub.Clients.Group("all-users").SendAsync("WorkOrderCreated", MapToDto(created!));

        return CreatedAtAction(nameof(GetById), new { id = wo.Id }, MapToDto(created!));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<WorkOrderDto>> Update(int id, [FromBody] UpdateWorkOrderDto dto)
    {
        var wo = await _uow.WorkOrders.GetByIdAsync(id);
        if (wo == null) return NotFound();

        if (wo.CreatedByUserId != CurrentUserId && CurrentRole != "Supervisor" && CurrentRole != "Admin")
            return Forbid();

        wo.Title = dto.Title;
        wo.Description = dto.Description;
        wo.Priority = dto.Priority;
        wo.System = dto.System;
        wo.DueDate = dto.DueDate;
        wo.AssignedToUserId = dto.AssignedToUserId;

        await _uow.WorkOrders.UpdateAsync(wo);
        var updated = await _uow.WorkOrders.GetByIdAsync(id);

        await _hub.Clients.Group("all-users").SendAsync("WorkOrderUpdated", MapToDto(updated!));

        return Ok(MapToDto(updated!));
    }

    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Supervisor,Admin")]
    public async Task<ActionResult<WorkOrderDto>> Approve(int id, [FromBody] ApprovalActionDto dto)
        => await TransitionStatus(id, WorkOrderStatus.Approved, dto.Action, dto.Notes);

    [HttpPost("{id}/reject")]
    [Authorize(Roles = "Supervisor,Admin")]
    public async Task<ActionResult<WorkOrderDto>> Reject(int id, [FromBody] ApprovalActionDto dto)
        => await TransitionStatus(id, WorkOrderStatus.Rejected, dto.Action, dto.Notes);

    [HttpPost("{id}/submit")]
    public async Task<ActionResult<WorkOrderDto>> Submit(int id)
        => await TransitionStatus(id, WorkOrderStatus.Submitted, "Submitted", null);

    [HttpPost("{id}/start")]
    [Authorize(Roles = "Engineer,Supervisor,Admin")]
    public async Task<ActionResult<WorkOrderDto>> Start(int id)
        => await TransitionStatus(id, WorkOrderStatus.InProgress, "Started", null);

    [HttpPost("{id}/complete")]
    public async Task<ActionResult<WorkOrderDto>> Complete(int id)
        => await TransitionStatus(id, WorkOrderStatus.Completed, "Completed", null);

    private async Task<ActionResult<WorkOrderDto>> TransitionStatus(
        int id, WorkOrderStatus newStatus, string action, string? notes)
    {
        var wo = await _uow.WorkOrders.GetByIdAsync(id);
        if (wo == null) return NotFound();

        wo.Status = newStatus;
        if (newStatus == WorkOrderStatus.Completed) wo.CompletedAt = DateTime.UtcNow;

        wo.ApprovalActions.Add(new ApprovalAction
        {
            WorkOrderId = id,
            UserId = CurrentUserId,
            Action = action,
            Notes = notes
        });

        await _uow.WorkOrders.UpdateAsync(wo);
        var updated = await _uow.WorkOrders.GetByIdAsync(id);

        await _hub.Clients.Group("all-users").SendAsync("WorkOrderStatusChanged", new
        {
            Id = id,
            NewStatus = newStatus.ToString(),
            Action = action,
            UpdatedBy = User.FindFirstValue(ClaimTypes.Name)
        });

        return Ok(MapToDto(updated!));
    }

    private static WorkOrderDto MapToDto(WorkOrder wo) => new(
        wo.Id,
        wo.Title,
        wo.Description,
        wo.Status,
        wo.Priority,
        wo.System,
        wo.CreatedBy?.Username ?? "Unknown",
        wo.AssignedTo?.Username,
        wo.CreatedAt,
        wo.DueDate,
        wo.CompletedAt
    );
}
