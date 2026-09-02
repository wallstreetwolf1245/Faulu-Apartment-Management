using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FauluApartmentAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantIdToMaintenanceOrder_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "MaintenanceOrders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceOrders_TenantId",
                table: "MaintenanceOrders",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceOrders_Tenants_TenantId",
                table: "MaintenanceOrders",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceOrders_Tenants_TenantId",
                table: "MaintenanceOrders");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceOrders_TenantId",
                table: "MaintenanceOrders");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "MaintenanceOrders");
        }
    }
}
