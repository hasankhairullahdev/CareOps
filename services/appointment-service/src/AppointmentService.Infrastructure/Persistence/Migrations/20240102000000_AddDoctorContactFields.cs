using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppointmentService.Infrastructure.Persistence.Migrations
{
    public partial class AddDoctorContactFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Doctors",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Doctors",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Doctors",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Doctors",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            // Back-fill seeded doctors with IsActive=true (CreatedAt gets default)
            migrationBuilder.Sql(
                "UPDATE \"Doctors\" SET \"IsActive\" = true, \"CreatedAt\" = '2024-01-01T00:00:00Z' WHERE \"CreatedAt\" = '-infinity' OR \"CreatedAt\" IS NULL;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn("CreatedAt", "Doctors");
            migrationBuilder.DropColumn("IsActive", "Doctors");
            migrationBuilder.DropColumn("Email", "Doctors");
            migrationBuilder.DropColumn("Phone", "Doctors");
        }
    }
}
