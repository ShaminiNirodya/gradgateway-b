using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GradGateway.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCoreAppTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Opportunities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OpportunityType = table.Column<int>(type: "int", nullable: false),
                    WorkMode = table.Column<int>(type: "int", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiredSkills = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MonthlyStipendLkr = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    DeadlineAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Opportunities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Opportunities_CompanyProfiles_CompanyProfileId",
                        column: x => x.CompanyProfileId,
                        principalTable: "CompanyProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Applications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OpportunityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CoverLetter = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AppliedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Applications_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "Opportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Applications_StudentProfiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalTable: "StudentProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Conversations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OpportunityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastMessageAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Conversations_CompanyProfiles_CompanyProfileId",
                        column: x => x.CompanyProfileId,
                        principalTable: "CompanyProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Conversations_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "Opportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Conversations_StudentProfiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalTable: "StudentProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SavedOpportunities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OpportunityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SavedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedOpportunities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavedOpportunities_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "Opportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SavedOpportunities_StudentProfiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalTable: "StudentProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SenderUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Messages_Users_SenderUserId",
                        column: x => x.SenderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Notifications",
                columns: new[] { "Id", "Body", "CreatedAt", "IsRead", "Title", "Type", "UserId" },
                values: new object[,]
                {
                    { new Guid("adadadad-1111-2222-3333-444444444444"), "Your application for Software Engineering Intern was submitted successfully.", new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Application Submitted", 1, new Guid("11111111-2222-3333-4444-555555555555") },
                    { new Guid("adadadad-1111-2222-3333-555555555555"), "You have been shortlisted for Data Analytics Intern at Dialog Axiata PLC.", new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Application Shortlisted", 1, new Guid("11111111-2222-3333-4444-666666666666") },
                    { new Guid("adadadad-1111-2222-3333-666666666666"), "You received a new message from Demo Student.", new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "New Candidate Message", 3, new Guid("66666666-7777-8888-9999-aaaaaaaaaaaa") }
                });

            migrationBuilder.InsertData(
                table: "Opportunities",
                columns: new[] { "Id", "CompanyProfileId", "CreatedAt", "DeadlineAt", "Description", "IsActive", "Location", "MonthlyStipendLkr", "OpportunityType", "RequiredSkills", "Title", "UpdatedAt", "WorkMode" },
                values: new object[,]
                {
                    { new Guid("dddddddd-1111-2222-3333-444444444444"), new Guid("cccccccc-1111-2222-3333-444444444444"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 4, 30, 0, 0, 0, 0, DateTimeKind.Utc), "Internship for 3rd year undergraduates with C# and React exposure.", true, "Colombo 03", 75000m, 0, "C#, ASP.NET Core, React, SQL Server", "Software Engineering Intern", new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { new Guid("dddddddd-1111-2222-3333-555555555555"), new Guid("cccccccc-1111-2222-3333-555555555555"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 5, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Hands-on analytics internship with telecom datasets and Power BI.", true, "Battaramulla", 85000m, 0, "Python, SQL, Power BI, Statistics", "Data Analytics Intern", new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0 },
                    { new Guid("dddddddd-1111-2222-3333-666666666666"), new Guid("cccccccc-1111-2222-3333-666666666666"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 6, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Entry-level role for fresh graduates interested in cloud-native development.", true, "Colombo 07", 180000m, 1, "Java, Microservices, Docker, Kubernetes", "Associate Software Engineer", new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { new Guid("dddddddd-1111-2222-3333-777777777777"), new Guid("cccccccc-1111-2222-3333-777777777777"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 5, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Internship focused on test automation for enterprise applications.", true, "Sri Lanka", 70000m, 0, "Selenium, Playwright, C#, CI/CD", "QA Automation Intern", new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2 }
                });

            migrationBuilder.InsertData(
                table: "Applications",
                columns: new[] { "Id", "AppliedAt", "CoverLetter", "OpportunityId", "Status", "StudentProfileId", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("eeeeeeee-1111-2222-3333-444444444444"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "I am eager to contribute to ASP.NET Core projects and learn from your team.", new Guid("dddddddd-1111-2222-3333-444444444444"), 0, new Guid("bbbbbbbb-1111-2222-3333-444444444444"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("eeeeeeee-1111-2222-3333-555555555555"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "My data analytics coursework and projects align with this opportunity.", new Guid("dddddddd-1111-2222-3333-555555555555"), 1, new Guid("bbbbbbbb-1111-2222-3333-555555555555"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("eeeeeeee-1111-2222-3333-666666666666"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "I have strong backend foundations and cloud fundamentals.", new Guid("dddddddd-1111-2222-3333-666666666666"), 0, new Guid("bbbbbbbb-1111-2222-3333-666666666666"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Conversations",
                columns: new[] { "Id", "CompanyProfileId", "CreatedAt", "LastMessageAt", "OpportunityId", "StudentProfileId" },
                values: new object[,]
                {
                    { new Guid("abababab-1111-2222-3333-444444444444"), new Guid("cccccccc-1111-2222-3333-444444444444"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("dddddddd-1111-2222-3333-444444444444"), new Guid("bbbbbbbb-1111-2222-3333-444444444444") },
                    { new Guid("abababab-1111-2222-3333-555555555555"), new Guid("cccccccc-1111-2222-3333-555555555555"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("dddddddd-1111-2222-3333-555555555555"), new Guid("bbbbbbbb-1111-2222-3333-555555555555") }
                });

            migrationBuilder.InsertData(
                table: "SavedOpportunities",
                columns: new[] { "Id", "OpportunityId", "SavedAt", "StudentProfileId" },
                values: new object[,]
                {
                    { new Guid("ffffffff-1111-2222-3333-444444444444"), new Guid("dddddddd-1111-2222-3333-555555555555"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("bbbbbbbb-1111-2222-3333-444444444444") },
                    { new Guid("ffffffff-1111-2222-3333-555555555555"), new Guid("dddddddd-1111-2222-3333-777777777777"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("bbbbbbbb-1111-2222-3333-777777777777") }
                });

            migrationBuilder.InsertData(
                table: "Messages",
                columns: new[] { "Id", "Content", "ConversationId", "IsRead", "SenderUserId", "SentAt" },
                values: new object[,]
                {
                    { new Guid("acacacac-1111-2222-3333-444444444444"), "Good morning, I submitted my application. Could you share the interview timeline?", new Guid("abababab-1111-2222-3333-444444444444"), true, new Guid("11111111-2222-3333-4444-555555555555"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("acacacac-1111-2222-3333-555555555555"), "Thanks! Shortlisting will be completed by next week.", new Guid("abababab-1111-2222-3333-444444444444"), true, new Guid("66666666-7777-8888-9999-aaaaaaaaaaaa"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("acacacac-1111-2222-3333-666666666666"), "Please upload your updated CV and transcript.", new Guid("abababab-1111-2222-3333-555555555555"), false, new Guid("66666666-7777-8888-9999-bbbbbbbbbbbb"), new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Applications_OpportunityId",
                table: "Applications",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_OpportunityId_StudentProfileId",
                table: "Applications",
                columns: new[] { "OpportunityId", "StudentProfileId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Applications_StudentProfileId",
                table: "Applications",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_CompanyProfileId",
                table: "Conversations",
                column: "CompanyProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_OpportunityId",
                table: "Conversations",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_StudentProfileId",
                table: "Conversations",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ConversationId",
                table: "Messages",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderUserId",
                table: "Messages",
                column: "SenderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SentAt",
                table: "Messages",
                column: "SentAt");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedAt",
                table: "Notifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_IsRead",
                table: "Notifications",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_CompanyProfileId",
                table: "Opportunities",
                column: "CompanyProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Opportunities_DeadlineAt",
                table: "Opportunities",
                column: "DeadlineAt");

            migrationBuilder.CreateIndex(
                name: "IX_SavedOpportunities_OpportunityId",
                table: "SavedOpportunities",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_SavedOpportunities_StudentProfileId",
                table: "SavedOpportunities",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_SavedOpportunities_StudentProfileId_OpportunityId",
                table: "SavedOpportunities",
                columns: new[] { "StudentProfileId", "OpportunityId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Applications");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "SavedOpportunities");

            migrationBuilder.DropTable(
                name: "Conversations");

            migrationBuilder.DropTable(
                name: "Opportunities");
        }
    }
}
