using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GradGateway.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProjectImagesToUseFirebaseUrls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "ProjectImages");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "ProjectImages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "ProjectImages");

            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "ProjectImages",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
