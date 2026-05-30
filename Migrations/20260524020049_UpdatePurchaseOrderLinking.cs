using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePurchaseOrderLinking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PRID",
                table: "PurchaseOrders",
                type: "integer",
                nullable: true);

            // Force add PRID to PurchaseRequisitions if somehow missing
            migrationBuilder.Sql("ALTER TABLE \"PurchaseRequisitions\" ADD COLUMN IF NOT EXISTS \"PRID\" SERIAL PRIMARY KEY;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PRID",
                table: "PurchaseOrders");
        }
    }
}
