using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MissionLog.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username     = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email        = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role         = table.Column<string>(type: "character varying(50)",  maxLength: 50,  nullable: false),
                    CreatedAt    = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive     = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_Users", x => x.Id));

            migrationBuilder.CreateTable(
                name: "WorkOrders",
                columns: table => new
                {
                    Id               = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title            = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description      = table.Column<string>(type: "text", nullable: false),
                    Status           = table.Column<int>(type: "integer", nullable: false),
                    Priority         = table.Column<int>(type: "integer", nullable: false),
                    System           = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedByUserId  = table.Column<int>(type: "integer", nullable: false),
                    AssignedToUserId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt        = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt        = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DueDate          = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt      = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrders", x => x.Id);
                    table.ForeignKey("FK_WorkOrders_Users_AssignedToUserId", x => x.AssignedToUserId, "Users", "Id", onDelete: ReferentialAction.SetNull);
                    table.ForeignKey("FK_WorkOrders_Users_CreatedByUserId",  x => x.CreatedByUserId,  "Users", "Id", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ApprovalActions",
                columns: table => new
                {
                    Id          = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkOrderId = table.Column<int>(type: "integer", nullable: false),
                    UserId      = table.Column<int>(type: "integer", nullable: false),
                    Action      = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Notes       = table.Column<string>(type: "text", nullable: true),
                    ActionDate  = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalActions", x => x.Id);
                    table.ForeignKey("FK_ApprovalActions_Users_UserId",           x => x.UserId,      "Users",      "Id", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey("FK_ApprovalActions_WorkOrders_WorkOrderId", x => x.WorkOrderId, "WorkOrders", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderComments",
                columns: table => new
                {
                    Id          = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkOrderId = table.Column<int>(type: "integer", nullable: false),
                    UserId      = table.Column<int>(type: "integer", nullable: false),
                    Content     = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt   = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderComments", x => x.Id);
                    table.ForeignKey("FK_WorkOrderComments_Users_UserId",           x => x.UserId,      "Users",      "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_WorkOrderComments_WorkOrders_WorkOrderId", x => x.WorkOrderId, "WorkOrders", "Id", onDelete: ReferentialAction.Cascade);
                });

            // Indexes
            migrationBuilder.CreateIndex("IX_Users_Email",                    "Users",              "Email",            unique: true);
            migrationBuilder.CreateIndex("IX_Users_Username",                 "Users",              "Username",         unique: true);
            migrationBuilder.CreateIndex("IX_WorkOrders_CreatedByUserId",     "WorkOrders",         "CreatedByUserId");
            migrationBuilder.CreateIndex("IX_WorkOrders_AssignedToUserId",    "WorkOrders",         "AssignedToUserId");
            migrationBuilder.CreateIndex("IX_ApprovalActions_WorkOrderId",    "ApprovalActions",    "WorkOrderId");
            migrationBuilder.CreateIndex("IX_ApprovalActions_UserId",         "ApprovalActions",    "UserId");
            migrationBuilder.CreateIndex("IX_WorkOrderComments_WorkOrderId",  "WorkOrderComments",  "WorkOrderId");
            migrationBuilder.CreateIndex("IX_WorkOrderComments_UserId",       "WorkOrderComments",  "UserId");

            // Demo users seeded at runtime via DatabaseSeeder (BCrypt requires runtime hashing)
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "WorkOrderComments");
            migrationBuilder.DropTable(name: "ApprovalActions");
            migrationBuilder.DropTable(name: "WorkOrders");
            migrationBuilder.DropTable(name: "Users");
        }
    }
}
