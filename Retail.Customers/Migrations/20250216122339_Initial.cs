using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Retail.Customers.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_customerNotifications",
                table: "customerNotifications");

            migrationBuilder.RenameTable(
                name: "customerNotifications",
                newName: "Notifications");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Notifications",
                table: "Notifications",
                column: "NotificationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Notifications",
                table: "Notifications");

            migrationBuilder.RenameTable(
                name: "Notifications",
                newName: "customerNotifications");

            migrationBuilder.AddPrimaryKey(
                name: "PK_customerNotifications",
                table: "customerNotifications",
                column: "NotificationId");
        }
    }
}
