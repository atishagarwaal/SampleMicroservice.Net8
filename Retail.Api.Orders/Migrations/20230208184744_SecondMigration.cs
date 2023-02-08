using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Retail.Api.Orders.Migrations
{
    /// <inheritdoc />
    public partial class SecondMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "Id", "CustomerId", "OrderDate", "TotalAmount" },
                values: new object[,]
                {
                    { 1L, 1L, new DateTime(2023, 2, 9, 0, 17, 43, 989, DateTimeKind.Local).AddTicks(1111), 80.0 },
                    { 2L, 2L, new DateTime(2023, 2, 9, 0, 17, 43, 989, DateTimeKind.Local).AddTicks(1141), 90.0 },
                    { 3L, 3L, new DateTime(2023, 2, 9, 0, 17, 43, 989, DateTimeKind.Local).AddTicks(1146), 140.0 }
                });

            migrationBuilder.InsertData(
                table: "LineItem",
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "LineItem",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "LineItem",
                keyColumn: "Id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "LineItem",
                keyColumn: "Id",
                keyValue: 3L);

            migrationBuilder.DeleteData(
                table: "LineItem",
                keyColumn: "Id",
                keyValue: 4L);

            migrationBuilder.DeleteData(
                table: "LineItem",
                keyColumn: "Id",
                keyValue: 5L);

            migrationBuilder.DeleteData(
                table: "LineItem",
                keyColumn: "Id",
                keyValue: 6L);

            migrationBuilder.DeleteData(
                table: "LineItem",
                keyColumn: "Id",
                keyValue: 7L);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 3L);
        }
    }
}
