using GradGateway.Data.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GradGateway.Data.Migrations
{
    [DbContext(typeof(GradGatewayDbContext))]
    [Migration("20260612160000_AddAdminSupportConversations")]
    public partial class AddAdminSupportConversations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'[Conversations]', N'Kind') IS NULL
    ALTER TABLE [Conversations] ADD [Kind] nvarchar(20) NOT NULL CONSTRAINT [DF_Conversations_Kind] DEFAULT 'StudentCompany';

IF COL_LENGTH(N'[Conversations]', N'SupportTargetUserId') IS NULL
    ALTER TABLE [Conversations] ADD [SupportTargetUserId] uniqueidentifier NULL;

IF COL_LENGTH(N'[Conversations]', N'StudentProfileId') IS NOT NULL
    ALTER TABLE [Conversations] ALTER COLUMN [StudentProfileId] uniqueidentifier NULL;

IF COL_LENGTH(N'[Conversations]', N'CompanyProfileId') IS NOT NULL
    ALTER TABLE [Conversations] ALTER COLUMN [CompanyProfileId] uniqueidentifier NULL;
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Conversations_Kind' AND object_id = OBJECT_ID(N'[Conversations]'))
    CREATE INDEX [IX_Conversations_Kind] ON [Conversations] ([Kind]);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Conversations_SupportTargetUserId' AND object_id = OBJECT_ID(N'[Conversations]'))
    CREATE INDEX [IX_Conversations_SupportTargetUserId] ON [Conversations] ([SupportTargetUserId]);

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = N'FK_Conversations_Users_SupportTargetUserId'
)
    ALTER TABLE [Conversations] WITH CHECK
    ADD CONSTRAINT [FK_Conversations_Users_SupportTargetUserId]
    FOREIGN KEY ([SupportTargetUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Conversations_Users_SupportTargetUserId')
    ALTER TABLE [Conversations] DROP CONSTRAINT [FK_Conversations_Users_SupportTargetUserId];

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Conversations_SupportTargetUserId' AND object_id = OBJECT_ID(N'[Conversations]'))
    DROP INDEX [IX_Conversations_SupportTargetUserId] ON [Conversations];

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Conversations_Kind' AND object_id = OBJECT_ID(N'[Conversations]'))
    DROP INDEX [IX_Conversations_Kind] ON [Conversations];

IF COL_LENGTH(N'[Conversations]', N'SupportTargetUserId') IS NOT NULL
    ALTER TABLE [Conversations] DROP COLUMN [SupportTargetUserId];

IF COL_LENGTH(N'[Conversations]', N'Kind') IS NOT NULL
BEGIN
    DECLARE @dfKind NVARCHAR(256);
    SELECT @dfKind = dc.name
    FROM sys.default_constraints dc
    JOIN sys.columns c ON c.default_object_id = dc.object_id
    WHERE dc.parent_object_id = OBJECT_ID(N'[Conversations]')
      AND c.name = N'Kind';
    IF @dfKind IS NOT NULL
        EXEC(N'ALTER TABLE [Conversations] DROP CONSTRAINT [' + @dfKind + N']');
    ALTER TABLE [Conversations] DROP COLUMN [Kind];
END
");
        }
    }
}
