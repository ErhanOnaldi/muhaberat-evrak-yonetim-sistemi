using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace muhaberat_evrak_yonetim.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCargoFieldsFromDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CargoTrackingLogs_Documents_DocumentId",
                table: "CargoTrackingLogs");

            migrationBuilder.DropIndex(
                name: "IX_Documents_CargoTrackingNumber",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_DeliveryStatus",
                table: "Documents");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Document_DeliveryStatus",
                table: "Documents");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Document_PackageType",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "CargoCompany",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "CargoTrackingNumber",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "DeliveryDate",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "DeliveryNotes",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "DeliveryStatus",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "EstimatedDeliveryDate",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "PackageType",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "ReceivedBy",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "ShippingAddress",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "ShippingDate",
                table: "Documents");

            migrationBuilder.AddColumn<int>(
                name: "CargoId",
                table: "Documents",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DocumentId",
                table: "CargoTrackingLogs",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "CargoId",
                table: "CargoTrackingLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Cargos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CargoTrackingNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CargoCompany = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ShippingAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeliveryAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PackageType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ShippingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstimatedDeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveryStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ReceivedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeliveryNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cargos", x => x.Id);
                    table.CheckConstraint("CK_Cargo_DeliveryStatus", "[DeliveryStatus] IN ('PREPARING', 'SHIPPED', 'IN_TRANSIT', 'DELIVERED', 'RETURNED')");
                    table.CheckConstraint("CK_Cargo_PackageType", "[PackageType] IS NULL OR [PackageType] IN ('ENVELOPE', 'SMALL_PACKAGE', 'LARGE_PACKAGE', 'SPECIAL')");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_CargoId",
                table: "Documents",
                column: "CargoId");

            migrationBuilder.CreateIndex(
                name: "IX_CargoTrackingLogs_CargoId",
                table: "CargoTrackingLogs",
                column: "CargoId");

            migrationBuilder.CreateIndex(
                name: "IX_Cargos_CargoTrackingNumber",
                table: "Cargos",
                column: "CargoTrackingNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Cargos_DeliveryStatus",
                table: "Cargos",
                column: "DeliveryStatus");

            migrationBuilder.AddForeignKey(
                name: "FK_CargoTrackingLogs_Cargos_CargoId",
                table: "CargoTrackingLogs",
                column: "CargoId",
                principalTable: "Cargos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CargoTrackingLogs_Documents_DocumentId",
                table: "CargoTrackingLogs",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Cargos_CargoId",
                table: "Documents",
                column: "CargoId",
                principalTable: "Cargos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CargoTrackingLogs_Cargos_CargoId",
                table: "CargoTrackingLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_CargoTrackingLogs_Documents_DocumentId",
                table: "CargoTrackingLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Cargos_CargoId",
                table: "Documents");

            migrationBuilder.DropTable(
                name: "Cargos");

            migrationBuilder.DropIndex(
                name: "IX_Documents_CargoId",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_CargoTrackingLogs_CargoId",
                table: "CargoTrackingLogs");

            migrationBuilder.DropColumn(
                name: "CargoId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "CargoId",
                table: "CargoTrackingLogs");

            migrationBuilder.AddColumn<string>(
                name: "CargoCompany",
                table: "Documents",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CargoTrackingNumber",
                table: "Documents",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress",
                table: "Documents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveryDate",
                table: "Documents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryNotes",
                table: "Documents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryStatus",
                table: "Documents",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "EstimatedDeliveryDate",
                table: "Documents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PackageType",
                table: "Documents",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReceivedBy",
                table: "Documents",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress",
                table: "Documents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ShippingDate",
                table: "Documents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DocumentId",
                table: "CargoTrackingLogs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_CargoTrackingNumber",
                table: "Documents",
                column: "CargoTrackingNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_DeliveryStatus",
                table: "Documents",
                column: "DeliveryStatus");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Document_DeliveryStatus",
                table: "Documents",
                sql: "[DeliveryStatus] IN ('PREPARING', 'SHIPPED', 'IN_TRANSIT', 'DELIVERED', 'RETURNED')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Document_PackageType",
                table: "Documents",
                sql: "[PackageType] IS NULL OR [PackageType] IN ('ENVELOPE', 'SMALL_PACKAGE', 'LARGE_PACKAGE', 'SPECIAL')");

            migrationBuilder.AddForeignKey(
                name: "FK_CargoTrackingLogs_Documents_DocumentId",
                table: "CargoTrackingLogs",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
