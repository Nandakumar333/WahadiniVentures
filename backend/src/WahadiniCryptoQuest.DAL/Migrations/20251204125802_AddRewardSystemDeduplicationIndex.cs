using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WahadiniCryptoQuest.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddRewardSystemDeduplicationIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Notified",
                table: "UserAchievements",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_reward_transactions_UserId_ReferenceId_ReferenceType",
                table: "reward_transactions",
                columns: new[] { "UserId", "ReferenceId", "ReferenceType" },
                unique: true,
                filter: "\"ReferenceId\" IS NOT NULL AND \"ReferenceType\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_reward_transactions_UserId_ReferenceId_ReferenceType",
                table: "reward_transactions");

            migrationBuilder.DropColumn(
                name: "Notified",
                table: "UserAchievements");
        }
    }
}
