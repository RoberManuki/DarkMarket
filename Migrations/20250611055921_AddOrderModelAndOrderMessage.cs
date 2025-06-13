using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DarkMarket.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderModelAndOrderMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Products",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveredAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FinishedAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentId1",
                table: "Orders",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Orders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "OrderMessage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PaymentId = table.Column<int>(type: "integer", nullable: false),
                    SenderUserId = table.Column<string>(type: "text", nullable: false),
                    SenderId = table.Column<string>(type: "text", nullable: true),
                    Content = table.Column<string>(type: "text", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OrderModelId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderMessage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderMessage_AspNetUsers_SenderId",
                        column: x => x.SenderId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderMessage_Orders_OrderModelId",
                        column: x => x.OrderModelId,
                        principalTable: "Orders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderMessage_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ProductId",
                table: "Payments",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PaymentId1",
                table: "Orders",
                column: "PaymentId1");

            migrationBuilder.CreateIndex(
                name: "IX_OrderMessage_OrderModelId",
                table: "OrderMessage",
                column: "OrderModelId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderMessage_PaymentId",
                table: "OrderMessage",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderMessage_SenderId",
                table: "OrderMessage",
                column: "SenderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Payments_PaymentId1",
                table: "Orders",
                column: "PaymentId1",
                principalTable: "Payments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Products_ProductId",
                table: "Payments",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Payments_PaymentId1",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Products_ProductId",
                table: "Payments");

            migrationBuilder.DropTable(
                name: "OrderMessage");

            migrationBuilder.DropIndex(
                name: "IX_Payments_ProductId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Orders_PaymentId1",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveredAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FinishedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaymentId1",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Orders");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Products",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);
        }
    }
}
