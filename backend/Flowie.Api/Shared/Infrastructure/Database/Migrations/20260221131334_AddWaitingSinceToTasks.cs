using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flowie.Api.Shared.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddWaitingSinceToTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "WaitingSince",
                table: "Tasks",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WaitingSince",
                table: "Tasks");
        }
    }
}
