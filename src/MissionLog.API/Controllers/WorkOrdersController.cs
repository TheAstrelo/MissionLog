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
    private string CurrentUsername => User.FindFirstValue(ClaimTypes.Name)!;

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
        var all = (await _uow.WorkOrders.GetAllAsync()).ToList();
        return Ok(new WorkOrderSummaryDto(
            Total:       all.Count,
            Draft:       all.Count(w => w.Status == WorkOrderStatus.Draft),
            UnderReview: all.Count(w => w.Status == WorkOrderStatus.UnderReview),
            Approved:    all.Count(w => w.Status == WorkOrderStatus.Approved),
            InProgress:  all.Count(w => w.Status == WorkOrderStatus.InProgress),
            Completed:   all.Count(w => w.Status == WorkOrderStatus.Completed),
            Rejected:    all.Count(w => w.Status == WorkOrderStatus.Rejected)
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
            Title           = dto.Title,
            Description     = dto.Description,
            Priority        = dto.Priority,
            System          = dto.System,
            DueDate         = dto.DueDate,
            CreatedByUserId = CurrentUserId,
            Status          = WorkOrderStatus.Draft
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

        wo.Title            = dto.Title;
        wo.Description      = dto.Description;
        wo.Priority         = dto.Priority;
        wo.System           = dto.System;
        wo.DueDate          = dto.DueDate;
        wo.AssignedToUserId = dto.AssignedToUserId;

        await _uow.WorkOrders.UpdateAsync(wo);
        var updated = await _uow.WorkOrders.GetByIdAsync(id);
        await _hub.Clients.Group("all-users").SendAsync("WorkOrderUpdated", MapToDto(updated!));
        return Ok(MapToDto(updated!));
    }

    // ── Workflow transitions ─────────────────────────────────────────────────

    [HttpPost("{id}/submit")]
    public async Task<ActionResult<WorkOrderDto>> Submit(int id)
        => await Transition(id, WorkOrderStatus.Submitted, "Submitted", null,
            allowed: wo => wo.Status == WorkOrderStatus.Draft);

    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Supervisor,Admin")]
    public async Task<ActionResult<WorkOrderDto>> Approve(int id, [FromBody] ApprovalActionDto dto)
        => await Transition(id, WorkOrderStatus.Approved, "Approved", dto.Notes,
            allowed: wo => wo.Status == WorkOrderStatus.Submitted || wo.Status == WorkOrderStatus.UnderReview);

    [HttpPost("{id}/reject")]
    [Authorize(Roles = "Supervisor,Admin")]
    public async Task<ActionResult<WorkOrderDto>> Reject(int id, [FromBody] ApprovalActionDto dto)
        => await Transition(id, WorkOrderStatus.Rejected, "Rejected", dto.Notes,
            allowed: wo => wo.Status == WorkOrderStatus.Submitted || wo.Status == WorkOrderStatus.UnderReview);

    [HttpPost("{id}/start")]
    [Authorize(Roles = "Engineer,Technician,Supervisor,Admin")]
    public async Task<ActionResult<WorkOrderDto>> Start(int id)
        => await Transition(id, WorkOrderStatus.InProgress, "Started", null,
            allowed: wo => wo.Status == WorkOrderStatus.Approved);

    [HttpPost("{id}/complete")]
    public async Task<ActionResult<WorkOrderDto>> Complete(int id)
        => await Transition(id, WorkOrderStatus.Completed, "Completed", null,
            allowed: wo => wo.Status == WorkOrderStatus.InProgress);

    // ── Shared transition helper ─────────────────────────────────────────────

    private async Task<ActionResult<WorkOrderDto>> Transition(
        int id,
        WorkOrderStatus newStatus,
        string action,
        string? notes,
        Func<WorkOrder, bool> allowed)
    {
        var wo = await _uow.WorkOrders.GetByIdAsync(id);
        if (wo == null) return NotFound();
        if (!allowed(wo)) return BadRequest(new { message = $"Cannot transition from {wo.Status} to {newStatus}." });

        wo.Status    = newStatus;
        wo.UpdatedAt = DateTime.UtcNow;
        if (newStatus == WorkOrderStatus.Completed) wo.CompletedAt = DateTime.UtcNow;

        wo.ApprovalActions.Add(new ApprovalAction
        {
            WorkOrderId = id,
            UserId      = CurrentUserId,
            Action      = action,
            Notes       = notes,
            ActionDate  = DateTime.UtcNow
        });

        await _uow.WorkOrders.UpdateAsync(wo);
        var updated = await _uow.WorkOrders.GetByIdAsync(id);

        await _hub.Clients.Group("all-users").SendAsync("WorkOrderStatusChanged", new
        {
            Id        = id,
            NewStatus = newStatus.ToString(),
            Action    = action,
            UpdatedBy = CurrentUsername
        });

        return Ok(MapToDto(updated!));
    }

    // ── Mapping ──────────────────────────────────────────────────────────────

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
        wo.CompletedAt,
        wo.ApprovalActions
            .OrderBy(a => a.ActionDate)
            .Select(a => new ApprovalActionDetailDto(
                a.Id,
                a.Action,
                a.Notes,
                a.User?.Username ?? "Unknown",
                a.ActionDate))
            .ToList()
    );
}
