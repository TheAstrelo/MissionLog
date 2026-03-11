using Microsoft.EntityFrameworkCore;
using MissionLog.Core.Entities;
using MissionLog.Core.Enums;

namespace MissionLog.Infrastructure.Data;

/// <summary>
/// Runs after migration on startup — seeds demo users and sample work orders
/// only if the database is empty. Safe to call on every startup (idempotent).
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Only seed if no users exist yet
        if (await context.Users.AnyAsync()) return;

        Console.WriteLine("[MissionLog] Seeding demo users...");

        var users = new List<User>
        {
            new()
            {
                Username    = "admin",
                Email       = "admin@missionlog.dev",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Role        = "Admin",
                CreatedAt   = DateTime.UtcNow,
                IsActive    = true
            },
            new()
            {
                Username    = "supervisor",
                Email       = "supervisor@missionlog.dev",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Super123!"),
                Role        = "Supervisor",
                CreatedAt   = DateTime.UtcNow,
                IsActive    = true
            },
            new()
            {
                Username    = "engineer",
                Email       = "engineer@missionlog.dev",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Eng123!"),
                Role        = "Engineer",
                CreatedAt   = DateTime.UtcNow,
                IsActive    = true
            },
            new()
            {
                Username    = "technician",
                Email       = "tech@missionlog.dev",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Tech123!"),
                Role        = "Technician",
                CreatedAt   = DateTime.UtcNow,
                IsActive    = true
            }
        };

        context.Users.AddRange(users);
        await context.SaveChangesAsync();

        Console.WriteLine("[MissionLog] Seeding sample work orders...");

        var admin      = users[0];
        var supervisor = users[1];
        var engineer   = users[2];
        var tech       = users[3];

        var workOrders = new List<WorkOrder>
        {
            new()
            {
                Title           = "Inspect hydraulic actuator on Bay 4 — pre-flight",
                Description     = "Pre-flight inspection of hydraulic actuator assembly. Check for leaks, verify pressure spec (3000 PSI ±50), inspect seals for wear. Document findings on Form ML-220.",
                Status          = WorkOrderStatus.Completed,
                Priority        = Priority.High,
                System          = "Hydraulics",
                CreatedByUserId = tech.Id,
                AssignedToUserId = engineer.Id,
                CreatedAt       = DateTime.UtcNow.AddDays(-10),
                CompletedAt     = DateTime.UtcNow.AddDays(-8),
                DueDate         = DateTime.UtcNow.AddDays(-8)
            },
            new()
            {
                Title           = "Replace avionics cooling fan — Unit 2B",
                Description     = "Cooling fan in avionics bay 2B has exceeded vibration threshold per sensor data. Replace with P/N AV-FAN-2200. Requires 2-hour ground power, coordinate with flight ops before starting.",
                Status          = WorkOrderStatus.InProgress,
                Priority        = Priority.Critical,
                System          = "Avionics",
                CreatedByUserId = engineer.Id,
                AssignedToUserId = tech.Id,
                CreatedAt       = DateTime.UtcNow.AddDays(-3),
                DueDate         = DateTime.UtcNow.AddDays(1)
            },
            new()
            {
                Title           = "Calibrate navigation transponder — annual",
                Description     = "Annual calibration of navigation transponder per maintenance schedule MS-1100. Requires calibration bench, test frequency sweep 1030-1090 MHz. Log results in NAVLOG.",
                Status          = WorkOrderStatus.Approved,
                Priority        = Priority.Medium,
                System          = "Navigation",
                CreatedByUserId = tech.Id,
                AssignedToUserId = engineer.Id,
                CreatedAt       = DateTime.UtcNow.AddDays(-5),
                DueDate         = DateTime.UtcNow.AddDays(7)
            },
            new()
            {
                Title           = "Repair comm antenna mount — corrosion detected",
                Description     = "Visual inspection found corrosion on antenna mount bracket at station 220. Clean, treat with Alodine 1201, apply sealant per SRM 51-71-00. Inspect bonding strap for continuity.",
                Status          = WorkOrderStatus.Submitted,
                Priority        = Priority.High,
                System          = "Communications",
                CreatedByUserId = tech.Id,
                CreatedAt       = DateTime.UtcNow.AddDays(-1),
                DueDate         = DateTime.UtcNow.AddDays(5)
            },
            new()
            {
                Title           = "Update flight management system software — v4.2.1",
                Description     = "FMS software update from v4.1.8 to v4.2.1 per service bulletin SB-FMS-2026-04. Backup current config, run update loader, verify all waypoints and performance data post-load. Estimated 45 min.",
                Status          = WorkOrderStatus.Draft,
                Priority        = Priority.Medium,
                System          = "Avionics",
                CreatedByUserId = engineer.Id,
                CreatedAt       = DateTime.UtcNow,
                DueDate         = DateTime.UtcNow.AddDays(14)
            },
            new()
            {
                Title           = "Thermal blanket replacement — aft equipment bay",
                Description     = "Thermal blanket in aft equipment bay showing degradation per last borescope inspection. Replace P/N TH-BLK-440A. Coordinate with structures team for access panel removal.",
                Status          = WorkOrderStatus.Rejected,
                Priority        = Priority.Low,
                System          = "Thermal Control",
                CreatedByUserId = tech.Id,
                CreatedAt       = DateTime.UtcNow.AddDays(-7),
                DueDate         = DateTime.UtcNow.AddDays(30)
            }
        };

        context.WorkOrders.AddRange(workOrders);
        await context.SaveChangesAsync();

        // Add approval actions to give history depth
        var approvals = new List<ApprovalAction>
        {
            // Completed WO history
            new() { WorkOrderId = workOrders[0].Id, UserId = tech.Id,       Action = "Submitted",  ActionDate = workOrders[0].CreatedAt.AddHours(1) },
            new() { WorkOrderId = workOrders[0].Id, UserId = supervisor.Id, Action = "Approved",   Notes = "Cleared for pre-flight. Priority confirmed.", ActionDate = workOrders[0].CreatedAt.AddHours(3) },
            new() { WorkOrderId = workOrders[0].Id, UserId = engineer.Id,   Action = "Completed",  Notes = "Inspection passed. All seals nominal, pressure at 2998 PSI.", ActionDate = workOrders[0].CompletedAt!.Value },

            // In-progress WO
            new() { WorkOrderId = workOrders[1].Id, UserId = engineer.Id,   Action = "Submitted",  ActionDate = workOrders[1].CreatedAt.AddHours(0.5) },
            new() { WorkOrderId = workOrders[1].Id, UserId = supervisor.Id, Action = "Approved",   Notes = "Critical — coordinate with ops center before ground power.", ActionDate = workOrders[1].CreatedAt.AddHours(2) },
            new() { WorkOrderId = workOrders[1].Id, UserId = tech.Id,       Action = "Started",    ActionDate = workOrders[1].CreatedAt.AddHours(4) },

            // Approved WO
            new() { WorkOrderId = workOrders[2].Id, UserId = tech.Id,       Action = "Submitted",  ActionDate = workOrders[2].CreatedAt.AddHours(1) },
            new() { WorkOrderId = workOrders[2].Id, UserId = supervisor.Id, Action = "Approved",   Notes = "Schedule with avionics shop.", ActionDate = workOrders[2].CreatedAt.AddHours(6) },

            // Submitted WO
            new() { WorkOrderId = workOrders[3].Id, UserId = tech.Id,       Action = "Submitted",  ActionDate = workOrders[3].CreatedAt.AddHours(0.5) },

            // Rejected WO
            new() { WorkOrderId = workOrders[5].Id, UserId = tech.Id,       Action = "Submitted",  ActionDate = workOrders[5].CreatedAt.AddHours(1) },
            new() { WorkOrderId = workOrders[5].Id, UserId = supervisor.Id, Action = "Rejected",   Notes = "Defer to next major maintenance cycle. Blanket still within service limits per QA.", ActionDate = workOrders[5].CreatedAt.AddHours(4) },
        };

        context.ApprovalActions.AddRange(approvals);
        await context.SaveChangesAsync();

        Console.WriteLine($"[MissionLog] Seed complete — {users.Count} users, {workOrders.Count} work orders, {approvals.Count} approval actions.");
    }
}
