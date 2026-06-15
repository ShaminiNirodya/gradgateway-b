using GradGateway.Data.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GradGateway.Data.Migrations
{
    [DbContext(typeof(GradGatewayDbContext))]
    [Migration("20260612190000_AddStudentHackathonsCompetitions")]
    public partial class AddStudentHackathonsCompetitions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[StudentEvents]', N'U') IS NOT NULL
    DROP TABLE [StudentEvents];

IF COL_LENGTH(N'StudentProfiles', N'HackathonsJson') IS NULL
    ALTER TABLE [StudentProfiles] ADD [HackathonsJson] nvarchar(max) NULL;

IF COL_LENGTH(N'StudentProfiles', N'CompetitionsJson') IS NULL
    ALTER TABLE [StudentProfiles] ADD [CompetitionsJson] nvarchar(max) NULL;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'StudentProfiles', N'CompetitionsJson') IS NOT NULL
    ALTER TABLE [StudentProfiles] DROP COLUMN [CompetitionsJson];

IF COL_LENGTH(N'StudentProfiles', N'HackathonsJson') IS NOT NULL
    ALTER TABLE [StudentProfiles] DROP COLUMN [HackathonsJson];
");
        }
    }
}
