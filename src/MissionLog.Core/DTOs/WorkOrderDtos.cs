using MissionLog.Core.Enums;

namespace MissionLog.Core.DTOs;

public record ApprovalActionDetailDto(
    int Id,
    string Action,
    string? Notes,
    string Username,
    DateTime ActionDate
);

public record WorkOrderDto(
    int Id,
    string Title,
    string Description,
    WorkOrderStatus Status,
    Priority Priority,
    string System,
    string CreatedBy,
    string? AssignedTo,
    DateTime CreatedAt,
    DateTime? DueDate,
    DateTime? CompletedAt,
    List<ApprovalActionDetailDto> ApprovalHistory
);

public record CreateWorkOrderDto(
    string Title,
    string Description,
    Priority Priority,
    string System,
    DateTime? DueDate
);

public record UpdateWorkOrderDto(
    string Title,
    string Description,
    Priority Priority,
    string System,
    DateTime? DueDate,
    int? AssignedToUserId
);

public record ApprovalActionDto(
    string Action,
    string? Notes
);

public record WorkOrderSummaryDto(
    int Total,
    int Draft,
    int UnderReview,
    int Approved,
    int InProgress,
    int Completed,
    int Rejected
);

public record UserDto(int Id, string Username, string Role);
