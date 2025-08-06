using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace muhaberat_evrak_yonetim.Migrations
{
    /// <inheritdoc />
    public partial class CreateDocumentTypeCategoriesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoryName",
                table: "DocumentTypes");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "DocumentTypes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DocumentTypeCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CategoryCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTypeCategories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_CategoryId",
                table: "DocumentTypes",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentTypes_DocumentTypeCategories_CategoryId",
                table: "DocumentTypes",
                column: "CategoryId",
                principalTable: "DocumentTypeCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentTypes_DocumentTypeCategories_CategoryId",
                table: "DocumentTypes");

            migrationBuilder.DropTable(
                name: "DocumentTypeCategories");

            migrationBuilder.DropIndex(
                name: "IX_DocumentTypes_CategoryId",
                table: "DocumentTypes");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "DocumentTypes");

            migrationBuilder.AddColumn<string>(
                name: "CategoryName",
                table: "DocumentTypes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
