using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FauluApartmentAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckoutRequestIdAndMerchantRequestIdToPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CheckoutRequestId",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MerchantRequestId",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PaymentReversals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentId = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    BuildingId = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaymentReferenceId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentReversals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentReversals_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentReversals_PaymentId",
                table: "PaymentReversals",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentReversals_RequestedAt",
                table: "PaymentReversals",
                column: "RequestedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentReversals_Status",
                table: "PaymentReversals",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentReversals");

            migrationBuilder.DropColumn(
                name: "CheckoutRequestId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "MerchantRequestId",
                table: "Payments");
        }
    }
}
