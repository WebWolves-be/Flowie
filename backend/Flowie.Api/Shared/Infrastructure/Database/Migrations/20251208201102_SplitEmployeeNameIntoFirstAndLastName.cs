using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flowie.Api.Migrations
{
    /// <inheritdoc />
    public partial class SplitEmployeeNameIntoFirstAndLastName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Employees");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Employees",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Employees",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Employees");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Employees",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");
        }
    }
}
