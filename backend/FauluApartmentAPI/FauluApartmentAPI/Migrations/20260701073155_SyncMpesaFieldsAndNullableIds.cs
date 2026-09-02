using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FauluApartmentAPI.Migrations
{
    /// <inheritdoc />
    public partial class SyncMpesaFieldsAndNullableIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "TenantId",
                table: "Payments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "LeaseId",
                table: "Payments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "BillRefNumber",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MpesaReceiptNumber",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayerName",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayerPhone",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentSource",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "UnitId",
                table: "Payments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_UnitId",
                table: "Payments",
                column: "UnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Units_UnitId",
                table: "Payments",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Units_UnitId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_UnitId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "BillRefNumber",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "MpesaReceiptNumber",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PayerName",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PayerPhone",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PaymentSource",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "UnitId",
                table: "Payments");

            migrationBuilder.AlterColumn<int>(
                name: "TenantId",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "LeaseId",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
