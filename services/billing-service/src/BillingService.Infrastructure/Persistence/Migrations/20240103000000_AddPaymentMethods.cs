using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillingService.Infrastructure.Persistence.Migrations
{
    public partial class AddPaymentMethods : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PaymentMethods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_PaymentMethods", x => x.Id));

            migrationBuilder.CreateIndex("IX_PaymentMethods_Name", "PaymentMethods", "Name", unique: true);

            migrationBuilder.InsertData(
                table: "PaymentMethods",
                columns: new[] { "Id", "Name", "Description", "IsActive", "CreatedAt" },
                values: new object[,]
                {
                    { new Guid("c0000000-0000-0000-0000-000000000001"), "Cash", null, true, new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("c0000000-0000-0000-0000-000000000002"), "Transfer Bank", null, true, new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("c0000000-0000-0000-0000-000000000003"), "BPJS", "Jaminan Kesehatan Nasional", true, new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("c0000000-0000-0000-0000-000000000004"), "Kartu Debit/Kredit", null, true, new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("PaymentMethods");
        }
    }
}
