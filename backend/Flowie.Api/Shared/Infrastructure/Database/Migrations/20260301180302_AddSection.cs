using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flowie.Api.Shared.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddSection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Create Sections table
            migrationBuilder.CreateTable(
                name: "Sections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sections_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sections_ProjectId",
                table: "Sections",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_ProjectId_Title",
                table: "Sections",
                columns: new[] { "ProjectId", "Title" },
                unique: true,
                filter: "[IsDeleted] = 0");

            // Step 2: Add SectionId column to Tasks (nullable initially)
            migrationBuilder.AddColumn<int>(
                name: "SectionId",
                table: "Tasks",
                type: "int",
                nullable: true);

            // Step 3: Insert default "All" section for every project
            migrationBuilder.Sql(@"
                INSERT INTO Sections (Title, Description, ProjectId, DisplayOrder, IsDeleted, CreatedAt, UpdatedAt)
                SELECT
                    'All',
                    NULL,
                    p.Id,
                    0,
                    0,
                    SYSDATETIMEOFFSET(),
                    NULL
                FROM Projects p
                WHERE NOT EXISTS (SELECT 1 FROM Sections s WHERE s.ProjectId = p.Id AND s.Title = 'All')
            ");

            // Step 4: Update all existing tasks to point to their project's "All" section
            migrationBuilder.Sql(@"
                UPDATE t
                SET t.SectionId = s.Id
                FROM Tasks t
                INNER JOIN Sections s ON s.ProjectId = t.ProjectId AND s.Title = 'All'
                WHERE t.SectionId IS NULL
            ");

            // Step 5: Make SectionId non-nullable
            migrationBuilder.AlterColumn<int>(
                name: "SectionId",
                table: "Tasks",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_SectionId",
                table: "Tasks",
                column: "SectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Sections_SectionId",
                table: "Tasks",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // Step 6: Drop ProjectId column and its foreign key/index from Tasks
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Projects_ProjectId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_ProjectId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "Tasks");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse Step 6: Add ProjectId column back to Tasks
            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "Tasks",
                type: "int",
                nullable: true);

            // Restore ProjectId from Section.ProjectId
            migrationBuilder.Sql(@"
                UPDATE t
                SET t.ProjectId = s.ProjectId
                FROM Tasks t
                INNER JOIN Sections s ON s.Id = t.SectionId
            ");

            migrationBuilder.AlterColumn<int>(
                name: "ProjectId",
                table: "Tasks",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ProjectId",
                table: "Tasks",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Projects_ProjectId",
                table: "Tasks",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // Reverse Step 5 & 2: Drop SectionId column
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Sections_SectionId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_SectionId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "SectionId",
                table: "Tasks");

            // Reverse Step 1: Drop Sections table
            migrationBuilder.DropTable(
                name: "Sections");
        }
    }
}
