using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillingService.Infrastructure.Persistence.Migrations
{
    public partial class AddServiceTariffs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServiceTariffs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table => table.PrimaryKey("PK_ServiceTariffs", x => x.Id));

            migrationBuilder.CreateIndex(
                name: "IX_ServiceTariffs_Category_ServiceName",
                table: "ServiceTariffs",
                columns: new[] { "Category", "ServiceName" });

            // Seed default consultation tariff
            migrationBuilder.InsertData(
                table: "ServiceTariffs",
                columns: new[] { "Id", "ServiceName", "Category", "Price", "Description", "IsActive", "CreatedAt", "UpdatedAt" },
                values: new object[] {
                    new Guid("b0000000-0000-0000-0000-000000000001"),
                    "Biaya Konsultasi Umum",
                    "Consultation",
                    150_000m,
                    null,
                    true,
                    new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    null
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("ServiceTariffs");
        }
    }
}
