using GradGateway.Data.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GradGateway.Data.Migrations
{
    [DbContext(typeof(GradGatewayDbContext))]
    [Migration("20260612210000_AddStudentProfileDirectoryIndexes")]
    public partial class AddStudentProfileDirectoryIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // nvarchar(max) cannot be indexed — shrink to bounded length first (separate batch).
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'StudentProfiles', N'University') IS NOT NULL
BEGIN
    DECLARE @univMaxLen int;
    SELECT @univMaxLen = CASE WHEN c.max_length = -1 THEN -1 ELSE c.max_length / 2 END
    FROM sys.columns c
    WHERE c.object_id = OBJECT_ID(N'StudentProfiles') AND c.name = N'University';

    IF @univMaxLen = -1
        ALTER TABLE [StudentProfiles] ALTER COLUMN [University] nvarchar(200) NOT NULL;
END
");

            migrationBuilder.Sql(@"
IF COL_LENGTH(N'StudentProfiles', N'Degree') IS NOT NULL
BEGIN
    DECLARE @degreeMaxLen int;
    SELECT @degreeMaxLen = CASE WHEN c.max_length = -1 THEN -1 ELSE c.max_length / 2 END
    FROM sys.columns c
    WHERE c.object_id = OBJECT_ID(N'StudentProfiles') AND c.name = N'Degree';

    IF @degreeMaxLen = -1
        ALTER TABLE [StudentProfiles] ALTER COLUMN [Degree] nvarchar(200) NOT NULL;
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StudentProfiles_UpdatedAt' AND object_id = OBJECT_ID(N'StudentProfiles'))
    CREATE INDEX [IX_StudentProfiles_UpdatedAt] ON [StudentProfiles] ([UpdatedAt]);
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StudentProfiles_University' AND object_id = OBJECT_ID(N'StudentProfiles'))
    CREATE INDEX [IX_StudentProfiles_University] ON [StudentProfiles] ([University]);
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StudentProfiles_GradYear' AND object_id = OBJECT_ID(N'StudentProfiles'))
    CREATE INDEX [IX_StudentProfiles_GradYear] ON [StudentProfiles] ([GradYear]);
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StudentProfiles_Gpa' AND object_id = OBJECT_ID(N'StudentProfiles'))
    CREATE INDEX [IX_StudentProfiles_Gpa] ON [StudentProfiles] ([Gpa]);
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Applications_AppliedAt' AND object_id = OBJECT_ID(N'Applications'))
    CREATE INDEX [IX_Applications_AppliedAt] ON [Applications] ([AppliedAt]);
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Applications_AppliedAt' AND object_id = OBJECT_ID(N'Applications'))
    DROP INDEX [IX_Applications_AppliedAt] ON [Applications];
");

            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StudentProfiles_Gpa' AND object_id = OBJECT_ID(N'StudentProfiles'))
    DROP INDEX [IX_StudentProfiles_Gpa] ON [StudentProfiles];
");

            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StudentProfiles_GradYear' AND object_id = OBJECT_ID(N'StudentProfiles'))
    DROP INDEX [IX_StudentProfiles_GradYear] ON [StudentProfiles];
");

            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StudentProfiles_University' AND object_id = OBJECT_ID(N'StudentProfiles'))
    DROP INDEX [IX_StudentProfiles_University] ON [StudentProfiles];
");

            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StudentProfiles_UpdatedAt' AND object_id = OBJECT_ID(N'StudentProfiles'))
    DROP INDEX [IX_StudentProfiles_UpdatedAt] ON [StudentProfiles];
");
        }
    }
}
