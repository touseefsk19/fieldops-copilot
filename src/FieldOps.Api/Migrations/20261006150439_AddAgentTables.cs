using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FieldOps.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAgentTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    User = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Input = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Result = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SpareParts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Equipment = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Bin = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpareParts", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "SpareParts",
                columns: new[] { "Id", "Bin", "Equipment", "Name", "PartNumber", "Quantity" },
                values: new object[,]
                {
                    { 1, "A-12", "CP-200 pump", "CP-200 mechanical seal kit", "SK-200", 3 },
                    { 2, "A-14", "CP-200 pump", "Coupling bolt set (M12)", "CB-45", 12 },
                    { 3, "A-15", "CP-200 pump", "Pump bearing 6205", "BRG-6205", 0 },
                    { 4, "C-03", "FL-30 forklift", "FL-30 lift motor cooling fan", "FAN-FL30", 2 },
                    { 5, "S-01", "LOTO", "Personal safety padlock", "LOTO-PL", 20 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditEntries_AtUtc",
                table: "AuditEntries",
                column: "AtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_SpareParts_PartNumber",
                table: "SpareParts",
                column: "PartNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditEntries");

            migrationBuilder.DropTable(
                name: "SpareParts");
        }
    }
}
