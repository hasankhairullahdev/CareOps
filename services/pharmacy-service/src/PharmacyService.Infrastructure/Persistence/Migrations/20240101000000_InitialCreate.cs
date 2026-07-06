using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmacyService.Infrastructure.Persistence.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable("Medicines", table => new
            {
                Id = table.Column<Guid>("uuid", nullable: false),
                Name = table.Column<string>("character varying(200)", maxLength: 200, nullable: false),
                GenericName = table.Column<string>("character varying(200)", maxLength: 200, nullable: false),
                Category = table.Column<string>("character varying(100)", maxLength: 100, nullable: false),
                Unit = table.Column<string>("character varying(20)", maxLength: 20, nullable: false),
                StockQuantity = table.Column<int>("integer", nullable: false),
                MinimumStock = table.Column<int>("integer", nullable: false),
                Price = table.Column<decimal>("decimal(18,2)", nullable: false),
                ExpiryDate = table.Column<DateOnly>("date", nullable: false)
            }, constraints: t => t.PrimaryKey("PK_Medicines", x => x.Id));

            migrationBuilder.CreateTable("Prescriptions", table => new
            {
                Id = table.Column<Guid>("uuid", nullable: false),
                ExternalPrescriptionId = table.Column<Guid>("uuid", nullable: false),
                PatientId = table.Column<Guid>("uuid", nullable: false),
                AppointmentId = table.Column<Guid>("uuid", nullable: false),
                Status = table.Column<string>("text", nullable: false),
                CreatedAt = table.Column<DateTime>("timestamp with time zone", nullable: false),
                DispensedAt = table.Column<DateTime>("timestamp with time zone", nullable: true)
            }, constraints: t => t.PrimaryKey("PK_Prescriptions", x => x.Id));

            migrationBuilder.CreateTable("PrescriptionItems", table => new
            {
                Id = table.Column<Guid>("uuid", nullable: false),
                PrescriptionId = table.Column<Guid>("uuid", nullable: false),
                MedicineId = table.Column<Guid>("uuid", nullable: true),
                MedicineName = table.Column<string>("character varying(200)", maxLength: 200, nullable: false),
                Quantity = table.Column<int>("integer", nullable: false),
                Dosage = table.Column<string>("character varying(100)", maxLength: 100, nullable: false),
                Instructions = table.Column<string>("character varying(500)", maxLength: 500, nullable: false)
            }, constraints: t =>
            {
                t.PrimaryKey("PK_PrescriptionItems", x => x.Id);
                t.ForeignKey("FK_PrescriptionItems_Prescriptions_PrescriptionId", x => x.PrescriptionId, "Prescriptions", "Id", onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateTable("StockMovements", table => new
            {
                Id = table.Column<Guid>("uuid", nullable: false),
                MedicineId = table.Column<Guid>("uuid", nullable: false),
                Type = table.Column<string>("text", nullable: false),
                Quantity = table.Column<int>("integer", nullable: false),
                Reason = table.Column<string>("character varying(500)", maxLength: 500, nullable: false),
                CreatedAt = table.Column<DateTime>("timestamp with time zone", nullable: false)
            }, constraints: t => t.PrimaryKey("PK_StockMovements", x => x.Id));

            migrationBuilder.CreateIndex("IX_Prescriptions_ExternalPrescriptionId", "Prescriptions", "ExternalPrescriptionId", unique: true);

            migrationBuilder.InsertData("Medicines",
                new[] { "Id", "Name", "GenericName", "Category", "Unit", "StockQuantity", "MinimumStock", "Price", "ExpiryDate" },
                new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), "Paracetamol 500mg", "Paracetamol", "Analgesic", "Tablet", 500, 50, 500m, new DateOnly(2026, 12, 31) },
                    { new Guid("00000000-0000-0000-0000-000000000002"), "Amoxicillin 500mg", "Amoxicillin", "Antibiotic", "Kapsul", 200, 30, 2500m, new DateOnly(2026, 6, 30) },
                    { new Guid("00000000-0000-0000-0000-000000000003"), "Omeprazole 20mg", "Omeprazole", "Antacid", "Kapsul", 150, 20, 3000m, new DateOnly(2026, 9, 30) },
                    { new Guid("00000000-0000-0000-0000-000000000004"), "Cetirizine 10mg", "Cetirizine", "Antihistamine", "Tablet", 10, 20, 1500m, new DateOnly(2025, 3, 31) },
                    { new Guid("00000000-0000-0000-0000-000000000005"), "Metformin 500mg", "Metformin", "Antidiabetic", "Tablet", 300, 50, 1000m, new DateOnly(2026, 12, 31) }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("PrescriptionItems");
            migrationBuilder.DropTable("Prescriptions");
            migrationBuilder.DropTable("StockMovements");
            migrationBuilder.DropTable("Medicines");
        }
    }
}
