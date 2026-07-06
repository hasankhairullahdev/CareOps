using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BillingService.Infrastructure.Persistence.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("Bills", table => new
            {
                Id = table.Column<Guid>("uuid", nullable: false),
                PatientId = table.Column<Guid>("uuid", nullable: false),
                AppointmentId = table.Column<Guid>("uuid", nullable: false),
                Status = table.Column<string>("text", nullable: false),
                TotalAmount = table.Column<decimal>("decimal(18,2)", nullable: false),
                CreatedAt = table.Column<DateTime>("timestamp with time zone", nullable: false),
                IssuedAt = table.Column<DateTime>("timestamp with time zone", nullable: true),
                PaidAt = table.Column<DateTime>("timestamp with time zone", nullable: true)
            }, constraints: t => t.PrimaryKey("PK_Bills", x => x.Id));

            migrationBuilder.CreateTable("BillLineItems", table => new
            {
                Id = table.Column<Guid>("uuid", nullable: false),
                BillId = table.Column<Guid>("uuid", nullable: false),
                Description = table.Column<string>("character varying(500)", maxLength: 500, nullable: false),
                Quantity = table.Column<int>("integer", nullable: false),
                UnitPrice = table.Column<decimal>("decimal(18,2)", nullable: false),
                Amount = table.Column<decimal>("decimal(18,2)", nullable: false)
            }, constraints: t =>
            {
                t.PrimaryKey("PK_BillLineItems", x => x.Id);
                t.ForeignKey("FK_BillLineItems_Bills_BillId", x => x.BillId, "Bills", "Id", onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateIndex("IX_Bills_AppointmentId", "Bills", "AppointmentId", unique: true);
            migrationBuilder.CreateIndex("IX_Bills_PatientId", "Bills", "PatientId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("BillLineItems");
            migrationBuilder.DropTable("Bills");
        }
    }
}
