using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WahadiniCryptoQuest.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminDashboardEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add ban-related columns to users table
            migrationBuilder.AddColumn<bool>(
                name: "IsBanned",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "BanReason",
                table: "users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BannedAt",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BannedBy",
                table: "users",
                type: "uuid",
                nullable: true);

            // Add admin-related columns to discount_codes table
            migrationBuilder.AddColumn<int>(
                name: "UsageLimit",
                table: "DiscountCodes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UsageCount",
                table: "DiscountCodes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "DiscountCodes",
                type: "uuid",
                nullable: true);

            // Create AuditLogEntries table
            migrationBuilder.CreateTable(
                name: "audit_log_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AdminUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ResourceType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ResourceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BeforeValue = table.Column<string>(type: "jsonb", nullable: true),
                    AfterValue = table.Column<string>(type: "jsonb", nullable: true),
                    IPAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_log_entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_audit_log_entries_users_AdminUserId",
                        column: x => x.AdminUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Create UserNotifications table
            migrationBuilder.CreateTable(
                name: "user_notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_notifications_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create PointAdjustments table
            migrationBuilder.CreateTable(
                name: "point_adjustments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreviousBalance = table.Column<int>(type: "integer", nullable: false),
                    AdjustmentAmount = table.Column<int>(type: "integer", nullable: false),
                    NewBalance = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    AdminUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_point_adjustments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_point_adjustments_users_AdminUserId",
                        column: x => x.AdminUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_point_adjustments_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Create indexes for AuditLogEntries
            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_AdminAction",
                table: "audit_log_entries",
                columns: new[] { "AdminUserId", "ActionType", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_Resource",
                table: "audit_log_entries",
                columns: new[] { "ResourceType", "ResourceId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_Timestamp",
                table: "audit_log_entries",
                column: "Timestamp");

            // Create indexes for UserNotifications
            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_UserUnread",
                table: "user_notifications",
                columns: new[] { "UserId", "IsRead", "CreatedAt" })
                .Annotation("Npgsql:IndexSortOrder", new[] { Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.SortOrder.Ascending, Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.SortOrder.Ascending, Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.SortOrder.Descending });

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_User",
                table: "user_notifications",
                column: "UserId");

            // Create indexes for PointAdjustments
            migrationBuilder.CreateIndex(
                name: "IX_PointAdjustments_UserHistory",
                table: "point_adjustments",
                columns: new[] { "UserId", "Timestamp" })
                .Annotation("Npgsql:IndexSortOrder", new[] { Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.SortOrder.Ascending, Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.SortOrder.Descending });

            migrationBuilder.CreateIndex(
                name: "IX_PointAdjustments_AdminHistory",
                table: "point_adjustments",
                columns: new[] { "AdminUserId", "Timestamp" });

            // Add indexes for analytics optimization (T032)
            migrationBuilder.CreateIndex(
                name: "IX_Users_CreatedAt",
                table: "users",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_Status_CreatedAt",
                table: "subscriptions",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_UserTaskSubmissions_Status",
                table: "UserTaskSubmissions",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop tables
            migrationBuilder.DropTable(name: "audit_log_entries");
            migrationBuilder.DropTable(name: "user_notifications");
            migrationBuilder.DropTable(name: "point_adjustments");

            // Drop indexes from users table
            migrationBuilder.DropIndex(name: "IX_Users_CreatedAt", table: "users");
            migrationBuilder.DropIndex(name: "IX_Subscriptions_Status_CreatedAt", table: "subscriptions");
            migrationBuilder.DropIndex(name: "IX_UserTaskSubmissions_Status", table: "UserTaskSubmissions");

            // Drop columns from users table
            migrationBuilder.DropColumn(name: "IsBanned", table: "users");
            migrationBuilder.DropColumn(name: "BanReason", table: "users");
            migrationBuilder.DropColumn(name: "BannedAt", table: "users");
            migrationBuilder.DropColumn(name: "BannedBy", table: "users");

            // Drop columns from discount_codes table
            migrationBuilder.DropColumn(name: "UsageLimit", table: "DiscountCodes");
            migrationBuilder.DropColumn(name: "UsageCount", table: "DiscountCodes");
            migrationBuilder.DropColumn(name: "CreatedBy", table: "DiscountCodes");
        }
    }
}
