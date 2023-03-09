using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Retail.Api.Orders.Migrations
{
    /// <inheritdoc />
    public partial class _001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<long>(type: "bigint", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalAmount = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LineItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    SkuId = table.Column<long>(type: "bigint", nullable: false),
                    Qty = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LineItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LineItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "Id", "CustomerId", "OrderDate", "TotalAmount" },
                values: new object[,]
                {
                    { 1L, 1L, new DateTime(2023, 3, 9, 13, 15, 21, 206, DateTimeKind.Local).AddTicks(503), 80.0 },
                    { 2L, 2L, new DateTime(2023, 3, 9, 13, 15, 21, 206, DateTimeKind.Local).AddTicks(515), 90.0 },
                    { 3L, 3L, new DateTime(2023, 3, 9, 13, 15, 21, 206, DateTimeKind.Local).AddTicks(516), 140.0 }
                });

            migrationBuilder.InsertData(
                table: "LineItems",
                columns: new[] { "Id", "OrderId", "Qty", "SkuId" },
                values: new object[,]
                {
                    { 1L, 1L, 1, 1L },
                    { 2L, 1L, 1, 2L },
                    { 3L, 2L, 1, 2L },
                    { 4L, 2L, 1, 3L },
                    { 5L, 3L, 1, 1L },
                    { 6L, 3L, 1, 2L },
                    { 7L, 3L, 1, 3L }
                });

            migrationBuilder.CreateIndex(
                name: "IX_LineItems_OrderId",
                table: "LineItems",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LineItems");

            migrationBuilder.DropTable(
                name: "Orders");
        }
    }
}
