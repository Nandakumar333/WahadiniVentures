using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WahadiniCryptoQuest.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddStripeSubscriptionV2 : Migration
    {
        /// <inheritdoc />
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Migration rendered empty to bypass "relation does not exist" errors.
            // The actual table creation/fixing is handled in 20251216000001_FixSubscriptionTables.cs
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No-op
        }
    }
}
