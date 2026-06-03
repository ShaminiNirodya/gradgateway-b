using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GradGateway.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentCvUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CvUrl",
                table: "StudentProfiles",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CvUrl",
                table: "StudentProfiles");
        }
    }
}
