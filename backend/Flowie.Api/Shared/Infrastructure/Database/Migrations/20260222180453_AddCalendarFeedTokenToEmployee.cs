using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flowie.Api.Shared.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddCalendarFeedTokenToEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CalendarFeedToken",
                table: "Employees",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_CalendarFeedToken",
                table: "Employees",
                column: "CalendarFeedToken",
                unique: true,
                filter: "[CalendarFeedToken] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Employees_CalendarFeedToken",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "CalendarFeedToken",
                table: "Employees");
        }
    }
}
