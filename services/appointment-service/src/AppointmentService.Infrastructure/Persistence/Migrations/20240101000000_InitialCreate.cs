using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppointmentService.Infrastructure.Persistence.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Doctors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Specialization = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LicenseNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Schedule = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_Doctors", x => x.Id));

            migrationBuilder.CreateTable(
                name: "Prescriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_Prescriptions", x => x.Id));

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.Id);
                    table.ForeignKey("FK_Appointments_Doctors_DoctorId", x => x.DoctorId, "Doctors", "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrescriptionItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PrescriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    MedicineName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Dosage = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Instructions = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrescriptionItems", x => x.Id);
                    table.ForeignKey("FK_PrescriptionItems_Prescriptions_PrescriptionId",
                        x => x.PrescriptionId, "Prescriptions", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex("IX_Appointments_DoctorId_ScheduledAt", "Appointments", new[] { "DoctorId", "ScheduledAt" });
            migrationBuilder.CreateIndex("IX_Appointments_PatientId", "Appointments", "PatientId");
            migrationBuilder.CreateIndex("IX_Doctors_LicenseNumber", "Doctors", "LicenseNumber", unique: true);

            // Seed doctors
            migrationBuilder.InsertData(
                table: "Doctors",
                columns: new[] { "Id", "Name", "Specialization", "LicenseNumber", "Schedule" },
                values: new object[,]
                {
                    { new Guid("d0000000-0000-0000-0000-000000000001"), "Dr. Andi Wirawan", "General Practice", "STR-001-2024", "Mon-Fri 08:00-16:00" },
                    { new Guid("d0000000-0000-0000-0000-000000000002"), "Dr. Sari Kusuma", "Internal Medicine", "STR-002-2024", "Mon-Fri 09:00-17:00" },
                    { new Guid("d0000000-0000-0000-0000-000000000003"), "Dr. Bima Prasetyo", "Pediatrics", "STR-003-2024", "Tue-Sat 08:00-15:00" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("PrescriptionItems");
            migrationBuilder.DropTable("Appointments");
            migrationBuilder.DropTable("Prescriptions");
            migrationBuilder.DropTable("Doctors");
        }
    }
}
