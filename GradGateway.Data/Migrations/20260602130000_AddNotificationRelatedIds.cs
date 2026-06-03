using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GradGateway.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationRelatedIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RelatedApplicationId",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RelatedConversationId",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RelatedStudentProfileId",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RelatedApplicationId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "RelatedConversationId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "RelatedStudentProfileId",
                table: "Notifications");
        }
    }
}
