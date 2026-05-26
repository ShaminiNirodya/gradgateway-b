using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GradGateway.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentHighlightsColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AwardsJson",
                table: "StudentProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CertificationsJson",
                table: "StudentProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "StudentProfiles",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-1111-2222-3333-444444444444"),
                columns: new[] { "AwardsJson", "CertificationsJson" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "StudentProfiles",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-1111-2222-3333-555555555555"),
                columns: new[] { "AwardsJson", "CertificationsJson" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "StudentProfiles",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-1111-2222-3333-666666666666"),
                columns: new[] { "AwardsJson", "CertificationsJson" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "StudentProfiles",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-1111-2222-3333-777777777777"),
                columns: new[] { "AwardsJson", "CertificationsJson" },
                values: new object[] { null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AwardsJson",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "CertificationsJson",
                table: "StudentProfiles");
        }
    }
}
