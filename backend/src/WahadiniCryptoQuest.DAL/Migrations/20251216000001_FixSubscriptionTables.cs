using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WahadiniCryptoQuest.DAL.Migrations
{
    /// <inheritdoc />
    public partial class FixSubscriptionTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Robustly drop potentially existing tables to ensure clean slate
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"CurrencyPricings\" CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"Subscriptions\" CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"SubscriptionHistories\" CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"WebhookEvents\" CASCADE;");
            
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"currency_pricings\" CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"subscriptions\" CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"subscription_histories\" CASCADE;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"webhook_events\" CASCADE;");

            migrationBuilder.CreateTable(
                name: "currency_pricings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    StripePriceId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    MonthlyPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    YearlyPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    YearlySavingsPercent = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    CurrencySymbol = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ShowDecimal = table.Column<bool>(type: "boolean", nullable: false),
                    DecimalPlaces = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_currency_pricings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "subscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    StripeCustomerId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    StripeSubscriptionId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    StripePriceId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Tier = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    CurrentPeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CurrentPeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsCancelledAtPeriodEnd = table.Column<bool>(type: "boolean", nullable: false),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_subscriptions_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "subscription_histories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChangeType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PreviousTier = table.Column<int>(type: "integer", nullable: true),
                    NewTier = table.Column<int>(type: "integer", nullable: true),
                    PreviousStatus = table.Column<int>(type: "integer", nullable: true),
                    NewStatus = table.Column<int>(type: "integer", nullable: true),
                    PreviousPeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NewPeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    TriggeredBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    WebhookEventId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscription_histories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_subscription_histories_subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "webhook_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StripeEventId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EventCreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FailedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PayloadJson = table.Column<string>(type: "text", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_webhook_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_webhook_events_subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_currency_pricings_CurrencyCode",
                table: "currency_pricings",
                column: "CurrencyCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_currency_pricings_IsActive",
                table: "currency_pricings",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_currency_pricings_StripePriceId",
                table: "currency_pricings",
                column: "StripePriceId");

            migrationBuilder.CreateIndex(
                name: "IX_subscription_histories_ChangeType",
                table: "subscription_histories",
                column: "ChangeType");

            migrationBuilder.CreateIndex(
                name: "IX_subscription_histories_SubscriptionId",
                table: "subscription_histories",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_subscription_histories_SubscriptionId_CreatedAt",
                table: "subscription_histories",
                columns: new[] { "SubscriptionId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_subscription_histories_TriggeredBy",
                table: "subscription_histories",
                column: "TriggeredBy",
                filter: "\"TriggeredBy\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_subscription_histories_WebhookEventId",
                table: "subscription_histories",
                column: "WebhookEventId",
                filter: "\"WebhookEventId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_CancelAtPeriodEnd_PeriodEnd",
                table: "subscriptions",
                columns: new[] { "IsCancelledAtPeriodEnd", "CurrentPeriodEnd" },
                filter: "\"IsCancelledAtPeriodEnd\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_Status",
                table: "subscriptions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_StripeCustomerId",
                table: "subscriptions",
                column: "StripeCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_StripeSubscriptionId",
                table: "subscriptions",
                column: "StripeSubscriptionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_UserId",
                table: "subscriptions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_UserId_Status",
                table: "subscriptions",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_webhook_events_EventType_CreatedAt",
                table: "webhook_events",
                columns: new[] { "EventType", "EventCreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_webhook_events_Status",
                table: "webhook_events",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_webhook_events_Status_RetryCount",
                table: "webhook_events",
                columns: new[] { "Status", "RetryCount" },
                filter: "\"Status\" = 3");

            migrationBuilder.CreateIndex(
                name: "IX_webhook_events_StripeEventId",
                table: "webhook_events",
                column: "StripeEventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_webhook_events_SubscriptionId",
                table: "webhook_events",
                column: "SubscriptionId",
                filter: "\"SubscriptionId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "currency_pricings");

            migrationBuilder.DropTable(
                name: "subscription_histories");

            migrationBuilder.DropTable(
                name: "webhook_events");

            migrationBuilder.DropTable(
                name: "subscriptions");
        }
    }
}
