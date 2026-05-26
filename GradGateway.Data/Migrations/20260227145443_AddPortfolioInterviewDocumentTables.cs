using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GradGateway.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPortfolioInterviewDocumentTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documents_StudentProfiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalTable: "StudentProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Interviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Mode = table.Column<int>(type: "int", nullable: false),
                    MeetingLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Interviews_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TechStack = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RepositoryUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DemoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projects_StudentProfiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalTable: "StudentProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Skills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skills", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StudentSkills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SkillId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProficiencyLevel = table.Column<int>(type: "int", nullable: false),
                    YearsOfExperience = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentSkills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentSkills_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentSkills_StudentProfiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalTable: "StudentProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Documents",
                columns: new[] { "Id", "FileName", "FileType", "FileUrl", "IsPublic", "StudentProfileId", "UploadedAt" },
                values: new object[,]
                {
                    { new Guid("b2b2b2b2-1111-2222-3333-444444444444"), "Demo_Student_CV.pdf", "CV", "https://storage.gradgateway.lk/docs/demo-student-cv.pdf", false, new Guid("bbbbbbbb-1111-2222-3333-444444444444"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b2b2b2b2-1111-2222-3333-555555555555"), "Nethmi_Perera_Transcript.pdf", "Transcript", "https://storage.gradgateway.lk/docs/nethmi-transcript.pdf", false, new Guid("bbbbbbbb-1111-2222-3333-555555555555"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b2b2b2b2-1111-2222-3333-666666666666"), "Sahan_Portfolio.pdf", "Portfolio", "https://storage.gradgateway.lk/docs/sahan-portfolio.pdf", true, new Guid("bbbbbbbb-1111-2222-3333-666666666666"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Interviews",
                columns: new[] { "Id", "ApplicationId", "CreatedAt", "Location", "MeetingLink", "Mode", "Notes", "ScheduledAt", "Status", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("b1b1b1b1-1111-2222-3333-444444444444"), new Guid("eeeeeeee-1111-2222-3333-444444444444"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "https://meet.google.com/demo-gradgateway-1", 0, "First-round technical interview", new DateTime(2026, 3, 10, 4, 0, 0, 0, DateTimeKind.Utc), 0, new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("b1b1b1b1-1111-2222-3333-555555555555"), new Guid("eeeeeeee-1111-2222-3333-555555555555"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Dialog HQ, Battaramulla", null, 1, "Case study and discussion", new DateTime(2026, 3, 12, 5, 30, 0, 0, DateTimeKind.Utc), 0, new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Projects",
                columns: new[] { "Id", "CreatedAt", "DemoUrl", "Description", "IsPublic", "RepositoryUrl", "StudentProfileId", "TechStack", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("aeaeaeae-1111-2222-3333-444444444444"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "https://portfolio.demo.lk/gradgateway", "A full-stack web platform for student recruitment workflow.", true, "https://github.com/demo/gradgateway-portfolio", new Guid("bbbbbbbb-1111-2222-3333-444444444444"), "Next.js, ASP.NET Core, SQL Server", "GradGateway Portfolio", new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("aeaeaeae-1111-2222-3333-555555555555"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Data analytics dashboard for Sri Lankan intercity bus utilization.", true, "https://github.com/demo/lanka-bus-analytics", new Guid("bbbbbbbb-1111-2222-3333-555555555555"), "Python, Power BI, PostgreSQL", "Lanka Bus Analytics", new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("aeaeaeae-1111-2222-3333-666666666666"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "https://eventhub.demo.lk", "Mobile-first event management system for university societies.", true, "https://github.com/demo/campus-event-hub", new Guid("bbbbbbbb-1111-2222-3333-666666666666"), "React Native, Firebase, Node.js", "Campus Event Hub", new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Skills",
                columns: new[] { "Id", "Category", "Name" },
                values: new object[,]
                {
                    { new Guid("afafafaf-1111-2222-3333-444444444444"), "Backend", "C#" },
                    { new Guid("afafafaf-1111-2222-3333-555555555555"), "Backend", "ASP.NET Core" },
                    { new Guid("afafafaf-1111-2222-3333-666666666666"), "Frontend", "React" },
                    { new Guid("afafafaf-1111-2222-3333-777777777777"), "Database", "SQL Server" },
                    { new Guid("afafafaf-1111-2222-3333-888888888888"), "Data", "Power BI" }
                });

            migrationBuilder.InsertData(
                table: "StudentSkills",
                columns: new[] { "Id", "ProficiencyLevel", "SkillId", "StudentProfileId", "YearsOfExperience" },
                values: new object[,]
                {
                    { new Guid("b0b0b0b0-1111-2222-3333-444444444444"), 2, new Guid("afafafaf-1111-2222-3333-444444444444"), new Guid("bbbbbbbb-1111-2222-3333-444444444444"), 2 },
                    { new Guid("b0b0b0b0-1111-2222-3333-555555555555"), 1, new Guid("afafafaf-1111-2222-3333-555555555555"), new Guid("bbbbbbbb-1111-2222-3333-444444444444"), 1 },
                    { new Guid("b0b0b0b0-1111-2222-3333-666666666666"), 2, new Guid("afafafaf-1111-2222-3333-888888888888"), new Guid("bbbbbbbb-1111-2222-3333-555555555555"), 2 },
                    { new Guid("b0b0b0b0-1111-2222-3333-777777777777"), 2, new Guid("afafafaf-1111-2222-3333-666666666666"), new Guid("bbbbbbbb-1111-2222-3333-666666666666"), 3 },
                    { new Guid("b0b0b0b0-1111-2222-3333-888888888888"), 1, new Guid("afafafaf-1111-2222-3333-777777777777"), new Guid("bbbbbbbb-1111-2222-3333-666666666666"), 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_FileType",
                table: "Documents",
                column: "FileType");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_StudentProfileId",
                table: "Documents",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Interviews_ApplicationId",
                table: "Interviews",
                column: "ApplicationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Interviews_ScheduledAt",
                table: "Interviews",
                column: "ScheduledAt");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_IsPublic",
                table: "Projects",
                column: "IsPublic");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_StudentProfileId",
                table: "Projects",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Skills_Name",
                table: "Skills",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentSkills_SkillId",
                table: "StudentSkills",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentSkills_StudentProfileId",
                table: "StudentSkills",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentSkills_StudentProfileId_SkillId",
                table: "StudentSkills",
                columns: new[] { "StudentProfileId", "SkillId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "Interviews");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "StudentSkills");

            migrationBuilder.DropTable(
                name: "Skills");
        }
    }
}
