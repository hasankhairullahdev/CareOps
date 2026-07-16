using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatientService.Infrastructure.Persistence.Migrations
{
    public partial class AddLookupTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BloodTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_BloodTypes", x => x.Id));

            migrationBuilder.CreateTable(
                name: "AllergyTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_AllergyTypes", x => x.Id));

            migrationBuilder.CreateIndex("IX_BloodTypes_Name", "BloodTypes", "Name", unique: true);
            migrationBuilder.CreateIndex("IX_AllergyTypes_Name", "AllergyTypes", "Name", unique: true);

            // Seed blood types
            migrationBuilder.InsertData("BloodTypes", new[] { "Id", "Name" }, new object[,]
            {
                { 1, "A+" }, { 2, "A-" },
                { 3, "B+" }, { 4, "B-" },
                { 5, "AB+" }, { 6, "AB-" },
                { 7, "O+" }, { 8, "O-" }
            });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("AllergyTypes");
            migrationBuilder.DropTable("BloodTypes");
        }
    }
}
