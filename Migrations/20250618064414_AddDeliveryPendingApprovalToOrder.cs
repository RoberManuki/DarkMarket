using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarkMarket.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryPendingApprovalToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DeliveryPendingApproval",
                table: "Orders",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryPendingApproval",
                table: "Orders");
        }
    }
}
