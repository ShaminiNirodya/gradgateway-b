using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GradGateway.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInterviewSchedulingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DeadlineNotificationSent",
                table: "Opportunities",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "RelatedOpportunityId",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Notifications",
                keyColumn: "Id",
                keyValue: new Guid("adadadad-1111-2222-3333-444444444444"),
                column: "RelatedOpportunityId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Notifications",
                keyColumn: "Id",
                keyValue: new Guid("adadadad-1111-2222-3333-555555555555"),
                column: "RelatedOpportunityId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Notifications",
                keyColumn: "Id",
                keyValue: new Guid("adadadad-1111-2222-3333-666666666666"),
                column: "RelatedOpportunityId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Opportunities",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-1111-2222-3333-444444444444"),
                column: "DeadlineNotificationSent",
                value: false);

            migrationBuilder.UpdateData(
                table: "Opportunities",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-1111-2222-3333-555555555555"),
                column: "DeadlineNotificationSent",
                value: false);

            migrationBuilder.UpdateData(
                table: "Opportunities",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-1111-2222-3333-666666666666"),
                column: "DeadlineNotificationSent",
                value: false);

            migrationBuilder.UpdateData(
                table: "Opportunities",
                keyColumn: "Id",
                keyValue: new Guid("dddddddd-1111-2222-3333-777777777777"),
                column: "DeadlineNotificationSent",
                value: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeadlineNotificationSent",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "RelatedOpportunityId",
                table: "Notifications");
        }
    }
}
