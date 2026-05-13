using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WahadiniCryptoQuest.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRowVersionForConcurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastActivityDate",
                table: "UserStreaks",
                newName: "LastLoginDate");

            migrationBuilder.AlterColumn<int>(
                name: "TotalPointsEarned",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "ReferralCode",
                table: "users",
                type: "character varying(6)",
                maxLength: 6,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CurrentPoints",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "users",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AdminUserId",
                table: "reward_transactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BalanceAfter",
                table: "reward_transactions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_reward_transactions_AdminUserId",
                table: "reward_transactions",
                column: "AdminUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_reward_transactions_users_AdminUserId",
                table: "reward_transactions",
                column: "AdminUserId",
                principalTable: "users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_reward_transactions_users_AdminUserId",
                table: "reward_transactions");

            migrationBuilder.DropIndex(
                name: "IX_reward_transactions_AdminUserId",
                table: "reward_transactions");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "users");

            migrationBuilder.DropColumn(
                name: "AdminUserId",
                table: "reward_transactions");

            migrationBuilder.DropColumn(
                name: "BalanceAfter",
                table: "reward_transactions");

            migrationBuilder.RenameColumn(
                name: "LastLoginDate",
                table: "UserStreaks",
                newName: "LastActivityDate");

            migrationBuilder.AlterColumn<int>(
                name: "TotalPointsEarned",
                table: "users",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "ReferralCode",
                table: "users",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(6)",
                oldMaxLength: 6,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CurrentPoints",
                table: "users",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);
        }
    }
}
