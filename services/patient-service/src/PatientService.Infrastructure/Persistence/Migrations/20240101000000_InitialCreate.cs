using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatientService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    Gender = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    MedicalRecordNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Patients_Email",
                table: "Patients",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Patients_MedicalRecordNumber",
                table: "Patients",
                column: "MedicalRecordNumber",
                unique: true);

            // Seed data
            migrationBuilder.InsertData(
                table: "Patients",
                columns: new[] { "Id", "FirstName", "LastName", "DateOfBirth", "Gender", "PhoneNumber", "Email", "Address", "MedicalRecordNumber", "CreatedAt" },
                values: new object[,]
                {
                    { new Guid("a1000000-0000-0000-0000-000000000001"), "Ahmad", "Santoso", new DateOnly(1985, 3, 15), "Male", "08123456789", "ahmad.santoso@email.com", "Jl. Sudirman No. 1, Jakarta", "MRN-20240101-1001", new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a1000000-0000-0000-0000-000000000002"), "Siti", "Rahayu", new DateOnly(1992, 7, 22), "Female", "08234567890", "siti.rahayu@email.com", "Jl. Gatot Subroto No. 5, Jakarta", "MRN-20240101-1002", new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a1000000-0000-0000-0000-000000000003"), "Budi", "Pratama", new DateOnly(1978, 11, 30), "Male", "08345678901", "budi.pratama@email.com", "Jl. Thamrin No. 10, Jakarta", "MRN-20240101-1003", new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a1000000-0000-0000-0000-000000000004"), "Dewi", "Lestari", new DateOnly(1995, 1, 8), "Female", "08456789012", "dewi.lestari@email.com", "Jl. Kuningan No. 3, Jakarta", "MRN-20240101-1004", new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a1000000-0000-0000-0000-000000000005"), "Rizky", "Hakim", new DateOnly(2000, 9, 18), "Male", "08567890123", "rizky.hakim@email.com", "Jl. Casablanca No. 7, Jakarta", "MRN-20240101-1005", new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Patients");
        }
    }
}
