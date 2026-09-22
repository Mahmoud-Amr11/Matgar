using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Matgar.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentIdempotency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_TransactionReference",
                table: "Payments");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IdempotencyKey",
                table: "Payments",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ProviderOrderId",
                table: "Payments",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderPaymentId",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_OrderId_IdempotencyKey",
                table: "Payments",
                columns: new[] { "OrderId", "IdempotencyKey" },
                unique: true,
                filter: "[IdempotencyKey] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_OrderId_IdempotencyKey",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ProviderOrderId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ProviderPaymentId",
                table: "Payments");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_TransactionReference",
                table: "Payments",
                column: "TransactionReference",
                unique: true,
                filter: "[TransactionReference] IS NOT NULL");
        }
    }
}
