using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GradGateway.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedMoreSriLankanData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FirebaseUid", "Role" },
                values: new object[,]
                {
                    { new Guid("11111111-2222-3333-4444-666666666666"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "nethmi.perera@eng.pdn.ac.lk", "demo-student-uid-002", 1 },
                    { new Guid("11111111-2222-3333-4444-777777777777"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "sahan.jayasinghe@stu.cmb.ac.lk", "demo-student-uid-003", 1 },
                    { new Guid("11111111-2222-3333-4444-888888888888"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "tharushi.senanayake@jfn.ac.lk", "demo-student-uid-004", 1 },
                    { new Guid("66666666-7777-8888-9999-bbbbbbbbbbbb"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "careers@dialog.lk", "demo-company-uid-002", 2 },
                    { new Guid("66666666-7777-8888-9999-cccccccccccc"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "internships@wso2.com", "demo-company-uid-003", 2 },
                    { new Guid("66666666-7777-8888-9999-dddddddddddd"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "hr@virtusa.com", "demo-company-uid-004", 2 }
                });

            migrationBuilder.InsertData(
                table: "CompanyProfiles",
                columns: new[] { "Id", "CompanyEmail", "CompanyName", "CreatedAt", "Industry", "LogoDataUrl", "Phone", "Position", "RecruiterEmail", "RecruiterName", "RecruiterPhone", "UpdatedAt", "UserId", "Website" },
                values: new object[,]
                {
                    { new Guid("cccccccc-1111-2222-3333-555555555555"), "talent@dialog.lk", "Dialog Axiata PLC", new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Telecommunications", null, "+94117722345", "Talent Acquisition Executive", "kasun.rodrigo@dialog.lk", "Kasun Rodrigo", "+94771239876", new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("66666666-7777-8888-9999-bbbbbbbbbbbb"), "https://www.dialog.lk" },
                    { new Guid("cccccccc-1111-2222-3333-666666666666"), "campus@wso2.com", "WSO2 Lanka (Pvt) Ltd", new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Software & IT", null, "+94112678901", "Campus Recruiter", "ishara.fernando@wso2.com", "Ishara Fernando", "+94712340987", new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("66666666-7777-8888-9999-cccccccccccc"), "https://wso2.com" },
                    { new Guid("cccccccc-1111-2222-3333-777777777777"), "university.relations@virtusa.com", "Virtusa (Pvt) Ltd", new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Software & IT", null, "+94117890123", "Associate Manager - Talent", "dinithi.abeywickrama@virtusa.com", "Dinithi Abeywickrama", "+94770123456", new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("66666666-7777-8888-9999-dddddddddddd"), "https://www.virtusa.com" }
                });

            migrationBuilder.InsertData(
                table: "StudentProfiles",
                columns: new[] { "Id", "CreatedAt", "Degree", "FullName", "Gpa", "GradYear", "Phone", "PhotoDataUrl", "StudentId", "University", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { new Guid("bbbbbbbb-1111-2222-3333-555555555555"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "BSc Engineering", "Nethmi Perera", 3.82m, 2026, "+94712223344", null, "PDN2023123", "University of Peradeniya", new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("11111111-2222-3333-4444-666666666666") },
                    { new Guid("bbbbbbbb-1111-2222-3333-666666666666"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "BSc (Hons) in Computer Science", "Sahan Jayasinghe", 3.67m, 2025, "+94773334455", null, "UOC2022110", "University of Colombo", new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("11111111-2222-3333-4444-777777777777") },
                    { new Guid("bbbbbbbb-1111-2222-3333-777777777777"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "BSc in Information Technology", "Tharushi Senanayake", 3.29m, 2027, "+94764445566", null, "UOJ2023567", "University of Jaffna", new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("11111111-2222-3333-4444-888888888888") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CompanyProfiles",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-1111-2222-3333-555555555555"));

            migrationBuilder.DeleteData(
                table: "CompanyProfiles",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-1111-2222-3333-666666666666"));

            migrationBuilder.DeleteData(
                table: "CompanyProfiles",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-1111-2222-3333-777777777777"));

            migrationBuilder.DeleteData(
                table: "StudentProfiles",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-1111-2222-3333-555555555555"));

            migrationBuilder.DeleteData(
                table: "StudentProfiles",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-1111-2222-3333-666666666666"));

            migrationBuilder.DeleteData(
                table: "StudentProfiles",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-1111-2222-3333-777777777777"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-2222-3333-4444-666666666666"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-2222-3333-4444-777777777777"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-2222-3333-4444-888888888888"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("66666666-7777-8888-9999-bbbbbbbbbbbb"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("66666666-7777-8888-9999-cccccccccccc"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("66666666-7777-8888-9999-dddddddddddd"));
        }
    }
}
