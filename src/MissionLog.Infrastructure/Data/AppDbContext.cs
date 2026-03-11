using Microsoft.EntityFrameworkCore;
using MissionLog.Core.Entities;

namespace MissionLog.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<ApprovalAction> ApprovalActions => Set<ApprovalAction>();
    public DbSet<WorkOrderComment> WorkOrderComments => Set<WorkOrderComment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Username).IsUnique();
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Username).HasMaxLength(100).IsRequired();
            e.Property(u => u.Email).HasMaxLength(200).IsRequired();
            e.Property(u => u.Role).HasMaxLength(50).IsRequired();
        });

        // WorkOrder
        modelBuilder.Entity<WorkOrder>(e =>
        {
            e.HasKey(w => w.Id);
            e.Property(w => w.Title).HasMaxLength(200).IsRequired();
            e.Property(w => w.System).HasMaxLength(100);

            e.HasOne(w => w.CreatedBy)
             .WithMany(u => u.AssignedWorkOrders)
             .HasForeignKey(w => w.CreatedByUserId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(w => w.AssignedTo)
             .WithMany()
             .HasForeignKey(w => w.AssignedToUserId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ApprovalAction
        modelBuilder.Entity<ApprovalAction>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Action).HasMaxLength(50).IsRequired();

            e.HasOne(a => a.WorkOrder)
             .WithMany(w => w.ApprovalActions)
             .HasForeignKey(a => a.WorkOrderId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(a => a.User)
             .WithMany(u => u.ApprovalActions)
             .HasForeignKey(a => a.UserId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // WorkOrderComment
        modelBuilder.Entity<WorkOrderComment>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Content).HasMaxLength(2000).IsRequired();

            e.HasOne(c => c.WorkOrder)
             .WithMany(w => w.Comments)
             .HasForeignKey(c => c.WorkOrderId)
             .OnDelete(DeleteBehavior.Cascade);
        });

    }
}
