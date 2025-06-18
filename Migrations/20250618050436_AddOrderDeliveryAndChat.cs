using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarkMarket.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderDeliveryAndChat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderMessage_AspNetUsers_SenderId",
                table: "OrderMessage");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderMessage_Orders_OrderModelId",
                table: "OrderMessage");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderMessage_Payments_PaymentId",
                table: "OrderMessage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderMessage",
                table: "OrderMessage");

            migrationBuilder.DropIndex(
                name: "IX_OrderMessage_OrderModelId",
                table: "OrderMessage");

            migrationBuilder.DropColumn(
                name: "OrderModelId",
                table: "OrderMessage");

            migrationBuilder.RenameTable(
                name: "OrderMessage",
                newName: "OrderMessages");

            migrationBuilder.RenameColumn(
                name: "SentAt",
                table: "OrderMessages",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "SenderUserId",
                table: "OrderMessages",
                newName: "UserRole");

            migrationBuilder.RenameColumn(
                name: "SenderId",
                table: "OrderMessages",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "PaymentId",
                table: "OrderMessages",
                newName: "OrderId");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "OrderMessages",
                newName: "Text");

            migrationBuilder.RenameIndex(
                name: "IX_OrderMessage_SenderId",
                table: "OrderMessages",
                newName: "IX_OrderMessages_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderMessage_PaymentId",
                table: "OrderMessages",
                newName: "IX_OrderMessages_OrderId");

            migrationBuilder.AddColumn<bool>(
                name: "FundsReleased",
                table: "Orders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDelivered",
                table: "Orders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderMessages",
                table: "OrderMessages",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderMessages_AspNetUsers_UserId",
                table: "OrderMessages",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderMessages_Orders_OrderId",
                table: "OrderMessages",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderMessages_AspNetUsers_UserId",
                table: "OrderMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderMessages_Orders_OrderId",
                table: "OrderMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderMessages",
                table: "OrderMessages");

            migrationBuilder.DropColumn(
                name: "FundsReleased",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsDelivered",
                table: "Orders");

            migrationBuilder.RenameTable(
                name: "OrderMessages",
                newName: "OrderMessage");

            migrationBuilder.RenameColumn(
                name: "UserRole",
                table: "OrderMessage",
                newName: "SenderUserId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "OrderMessage",
                newName: "SenderId");

            migrationBuilder.RenameColumn(
                name: "Text",
                table: "OrderMessage",
                newName: "Content");

            migrationBuilder.RenameColumn(
                name: "OrderId",
                table: "OrderMessage",
                newName: "PaymentId");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "OrderMessage",
                newName: "SentAt");

            migrationBuilder.RenameIndex(
                name: "IX_OrderMessages_UserId",
                table: "OrderMessage",
                newName: "IX_OrderMessage_SenderId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderMessages_OrderId",
                table: "OrderMessage",
                newName: "IX_OrderMessage_PaymentId");

            migrationBuilder.AddColumn<int>(
                name: "OrderModelId",
                table: "OrderMessage",
                type: "integer",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderMessage",
                table: "OrderMessage",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_OrderMessage_OrderModelId",
                table: "OrderMessage",
                column: "OrderModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderMessage_AspNetUsers_SenderId",
                table: "OrderMessage",
                column: "SenderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderMessage_Orders_OrderModelId",
                table: "OrderMessage",
                column: "OrderModelId",
                principalTable: "Orders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderMessage_Payments_PaymentId",
                table: "OrderMessage",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
