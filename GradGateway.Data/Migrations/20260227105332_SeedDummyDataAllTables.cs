using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GradGateway.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedDummyDataAllTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7890-1234-567890abcdef"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FirebaseUid", "Role" },
                values: new object[,]
                {
                    { new Guid("11111111-2222-3333-4444-555555555555"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "student.demo@uom.lk", "demo-student-uid-001", 1 },
                    { new Guid("66666666-7777-8888-9999-aaaaaaaaaaaa"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "company.demo@sample.lk", "demo-company-uid-001", 2 }
                });

            migrationBuilder.InsertData(
                table: "CompanyProfiles",
                columns: new[] { "Id", "CompanyEmail", "CompanyName", "CreatedAt", "Industry", "LogoDataUrl", "Phone", "Position", "RecruiterEmail", "RecruiterName", "RecruiterPhone", "UpdatedAt", "UserId", "Website" },
                values: new object[] { new Guid("cccccccc-1111-2222-3333-444444444444"), "careers@demotech.lk", "Demo Technologies (Pvt) Ltd", new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Software & IT", null, "+94114567890", "HR Manager", "anjana@demotech.lk", "Anjana Silva", "+94770111222", new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("66666666-7777-8888-9999-aaaaaaaaaaaa"), "https://demotech.lk" });

            migrationBuilder.InsertData(
                table: "StudentProfiles",
                columns: new[] { "Id", "CreatedAt", "Degree", "FullName", "Gpa", "GradYear", "Phone", "PhotoDataUrl", "StudentId", "University", "UpdatedAt", "UserId" },
                values: new object[] { new Guid("bbbbbbbb-1111-2222-3333-444444444444"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "BSc (Hons) in IT", "Demo Student", 3.45m, 2027, "+94771234567", null, "UOM2024001", "University of Moratuwa", new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("11111111-2222-3333-4444-555555555555") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CompanyProfiles",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-1111-2222-3333-444444444444"));

            migrationBuilder.DeleteData(
                table: "StudentProfiles",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-1111-2222-3333-444444444444"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-2222-3333-4444-555555555555"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("66666666-7777-8888-9999-aaaaaaaaaaaa"));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7890-1234-567890abcdef"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 27, 9, 36, 19, 939, DateTimeKind.Utc).AddTicks(9223));
        }
    }
}
