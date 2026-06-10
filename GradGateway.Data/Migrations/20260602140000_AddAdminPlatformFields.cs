using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GradGateway.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminPlatformFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "VerificationStatus",
                table: "CompanyProfiles",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "VerificationRejectionReason",
                table: "CompanyProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerifiedAt",
                table: "CompanyProfiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PlatformSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AllowRegistration = table.Column<bool>(type: "bit", nullable: false),
                    RequireCompanyVerification = table.Column<bool>(type: "bit", nullable: false),
                    MaintenanceMode = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformSettings", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "PlatformSettings",
                columns: new[] { "Id", "AllowRegistration", "MaintenanceMode", "RequireCompanyVerification", "UpdatedAt" },
                values: new object[] { new Guid("f0f0f0f0-1111-2222-3333-444444444444"), true, false, true, new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "CompanyProfiles",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-1111-2222-3333-444444444444"),
                columns: new[] { "VerificationStatus", "VerifiedAt" },
                values: new object[] { 1, new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "CompanyProfiles",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-1111-2222-3333-555555555555"),
                columns: new[] { "VerificationStatus", "VerifiedAt" },
                values: new object[] { 1, new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "CompanyProfiles",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-1111-2222-3333-666666666666"),
                columns: new[] { "VerificationStatus", "VerifiedAt" },
                values: new object[] { 1, new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "CompanyProfiles",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-1111-2222-3333-777777777777"),
                columns: new[] { "VerificationStatus", "VerifiedAt" },
                values: new object[] { 1, new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7890-1234-567890abcdef"),
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-2222-3333-4444-555555555555"),
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-2222-3333-4444-666666666666"),
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-2222-3333-4444-777777777777"),
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-2222-3333-4444-888888888888"),
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("66666666-7777-8888-9999-aaaaaaaaaaaa"),
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("66666666-7777-8888-9999-bbbbbbbbbbbb"),
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("66666666-7777-8888-9999-cccccccccccc"),
                column: "IsActive",
                value: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("66666666-7777-8888-9999-dddddddddddd"),
                column: "IsActive",
                value: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlatformSettings");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "VerificationStatus",
                table: "CompanyProfiles");

            migrationBuilder.DropColumn(
                name: "VerificationRejectionReason",
                table: "CompanyProfiles");

            migrationBuilder.DropColumn(
                name: "VerifiedAt",
                table: "CompanyProfiles");
        }
    }
}
