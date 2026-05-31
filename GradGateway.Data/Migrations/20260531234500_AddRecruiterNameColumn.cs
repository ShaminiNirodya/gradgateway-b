using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GradGateway.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRecruiterNameColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add RecruiterName column to CompanyProfiles table if it doesn't exist
            migrationBuilder.AddColumn<string>(
                name: "RecruiterName",
                table: "CompanyProfiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove RecruiterName column from CompanyProfiles table
            migrationBuilder.DropColumn(
                name: "RecruiterName",
                table: "CompanyProfiles");
        }
    }
}
