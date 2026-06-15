using GradGateway.Data.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GradGateway.Data.Migrations
{
    [DbContext(typeof(GradGatewayDbContext))]
    [Migration("20260612200000_MergeHackathonsCompetitionsColumn")]
    public partial class MergeHackathonsCompetitionsColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'StudentProfiles', N'HackathonsCompetitionsJson') IS NULL
    ALTER TABLE [StudentProfiles] ADD [HackathonsCompetitionsJson] nvarchar(max) NULL;

IF COL_LENGTH(N'StudentProfiles', N'HackathonsJson') IS NOT NULL
   OR COL_LENGTH(N'StudentProfiles', N'CompetitionsJson') IS NOT NULL
BEGIN
    UPDATE [StudentProfiles]
    SET [HackathonsCompetitionsJson] =
        CASE
            WHEN NULLIF(LTRIM(RTRIM(ISNULL([HackathonsJson], N''))), N'') IS NOT NULL
             AND NULLIF(LTRIM(RTRIM(ISNULL([CompetitionsJson], N''))), N'') IS NOT NULL
                THEN LEFT([HackathonsJson], LEN([HackathonsJson]) - 1) + N',' + SUBSTRING([CompetitionsJson], 2, LEN([CompetitionsJson]))
            WHEN NULLIF(LTRIM(RTRIM(ISNULL([HackathonsJson], N''))), N'') IS NOT NULL
                THEN [HackathonsJson]
            ELSE [CompetitionsJson]
        END
    WHERE [HackathonsCompetitionsJson] IS NULL
      AND (
            NULLIF(LTRIM(RTRIM(ISNULL([HackathonsJson], N''))), N'') IS NOT NULL
         OR NULLIF(LTRIM(RTRIM(ISNULL([CompetitionsJson], N''))), N'') IS NOT NULL
      );
END

IF COL_LENGTH(N'StudentProfiles', N'CompetitionsJson') IS NOT NULL
    ALTER TABLE [StudentProfiles] DROP COLUMN [CompetitionsJson];

IF COL_LENGTH(N'StudentProfiles', N'HackathonsJson') IS NOT NULL
    ALTER TABLE [StudentProfiles] DROP COLUMN [HackathonsJson];
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'StudentProfiles', N'HackathonsJson') IS NULL
    ALTER TABLE [StudentProfiles] ADD [HackathonsJson] nvarchar(max) NULL;

IF COL_LENGTH(N'StudentProfiles', N'CompetitionsJson') IS NULL
    ALTER TABLE [StudentProfiles] ADD [CompetitionsJson] nvarchar(max) NULL;

IF COL_LENGTH(N'StudentProfiles', N'HackathonsCompetitionsJson') IS NOT NULL
BEGIN
    UPDATE [StudentProfiles]
    SET [HackathonsJson] = [HackathonsCompetitionsJson]
    WHERE [HackathonsCompetitionsJson] IS NOT NULL;

    ALTER TABLE [StudentProfiles] DROP COLUMN [HackathonsCompetitionsJson];
END
");
        }
    }
}
