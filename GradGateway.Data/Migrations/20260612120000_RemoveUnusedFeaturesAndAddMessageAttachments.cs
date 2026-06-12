using GradGateway.Data.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GradGateway.Data.Migrations
{
    /// <summary>
    /// Cleanup migration:
    /// - Drops the unused SavedOpportunities and CompanyTeamMembers tables.
    /// - Drops the company verification columns (feature removed).
    /// - Drops PlatformSettings.RequireCompanyVerification (feature removed).
    /// - Adds Message attachment columns.
    /// - Ensures SupportInquiries.SubmitterRole exists (was previously added by a manual script).
    /// All operations are guarded so the migration applies cleanly whether or not
    /// earlier manual SQL scripts were run against the database.
    /// </summary>
    [DbContext(typeof(GradGatewayDbContext))]
    [Migration("20260612120000_RemoveUnusedFeaturesAndAddMessageAttachments")]
    public partial class RemoveUnusedFeaturesAndAddMessageAttachments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[SavedOpportunities]', N'U') IS NOT NULL
    DROP TABLE [SavedOpportunities];
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[CompanyTeamMembers]', N'U') IS NOT NULL
    DROP TABLE [CompanyTeamMembers];
");

            migrationBuilder.Sql(@"
IF COL_LENGTH(N'[CompanyProfiles]', N'VerificationStatus') IS NOT NULL
BEGIN
    DECLARE @dfVer NVARCHAR(256);
    SELECT @dfVer = dc.name
    FROM sys.default_constraints dc
    JOIN sys.columns c ON c.default_object_id = dc.object_id
    WHERE dc.parent_object_id = OBJECT_ID(N'[CompanyProfiles]')
      AND c.name = N'VerificationStatus';
    IF @dfVer IS NOT NULL
        EXEC(N'ALTER TABLE [CompanyProfiles] DROP CONSTRAINT [' + @dfVer + N']');
    ALTER TABLE [CompanyProfiles] DROP COLUMN [VerificationStatus];
END
IF COL_LENGTH(N'[CompanyProfiles]', N'VerificationRejectionReason') IS NOT NULL
    ALTER TABLE [CompanyProfiles] DROP COLUMN [VerificationRejectionReason];
IF COL_LENGTH(N'[CompanyProfiles]', N'VerifiedAt') IS NOT NULL
    ALTER TABLE [CompanyProfiles] DROP COLUMN [VerifiedAt];
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[PlatformSettings]', N'U') IS NOT NULL
   AND COL_LENGTH(N'[PlatformSettings]', N'RequireCompanyVerification') IS NOT NULL
BEGIN
    DECLARE @df NVARCHAR(256);
    SELECT @df = dc.name
    FROM sys.default_constraints dc
    JOIN sys.columns c ON c.default_object_id = dc.object_id
    WHERE dc.parent_object_id = OBJECT_ID(N'[PlatformSettings]')
      AND c.name = N'RequireCompanyVerification';
    IF @df IS NOT NULL
        EXEC(N'ALTER TABLE [PlatformSettings] DROP CONSTRAINT [' + @df + N']');
    ALTER TABLE [PlatformSettings] DROP COLUMN [RequireCompanyVerification];
END
");

            migrationBuilder.Sql(@"
IF COL_LENGTH(N'[Messages]', N'AttachmentUrl') IS NULL
    ALTER TABLE [Messages] ADD [AttachmentUrl] NVARCHAR(MAX) NULL;
IF COL_LENGTH(N'[Messages]', N'AttachmentName') IS NULL
    ALTER TABLE [Messages] ADD [AttachmentName] NVARCHAR(MAX) NULL;
IF COL_LENGTH(N'[Messages]', N'AttachmentType') IS NULL
    ALTER TABLE [Messages] ADD [AttachmentType] NVARCHAR(MAX) NULL;
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[SupportInquiries]', N'U') IS NOT NULL
   AND COL_LENGTH(N'[SupportInquiries]', N'SubmitterRole') IS NULL
    ALTER TABLE [SupportInquiries] ADD [SubmitterRole] NVARCHAR(20) NULL;
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[NewsletterSubscribers]', N'U') IS NOT NULL
    DROP TABLE [NewsletterSubscribers];
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'[Messages]', N'AttachmentUrl') IS NOT NULL
    ALTER TABLE [Messages] DROP COLUMN [AttachmentUrl];
IF COL_LENGTH(N'[Messages]', N'AttachmentName') IS NOT NULL
    ALTER TABLE [Messages] DROP COLUMN [AttachmentName];
IF COL_LENGTH(N'[Messages]', N'AttachmentType') IS NOT NULL
    ALTER TABLE [Messages] DROP COLUMN [AttachmentType];
");

            migrationBuilder.Sql(@"
IF COL_LENGTH(N'[CompanyProfiles]', N'VerificationStatus') IS NULL
    ALTER TABLE [CompanyProfiles] ADD [VerificationStatus] INT NOT NULL CONSTRAINT [DF_CompanyProfiles_VerificationStatus] DEFAULT 1;
IF COL_LENGTH(N'[CompanyProfiles]', N'VerificationRejectionReason') IS NULL
    ALTER TABLE [CompanyProfiles] ADD [VerificationRejectionReason] NVARCHAR(MAX) NULL;
IF COL_LENGTH(N'[CompanyProfiles]', N'VerifiedAt') IS NULL
    ALTER TABLE [CompanyProfiles] ADD [VerifiedAt] DATETIME2 NULL;
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[PlatformSettings]', N'U') IS NOT NULL
   AND COL_LENGTH(N'[PlatformSettings]', N'RequireCompanyVerification') IS NULL
    ALTER TABLE [PlatformSettings] ADD [RequireCompanyVerification] BIT NOT NULL CONSTRAINT [DF_PlatformSettings_RequireCompanyVerification] DEFAULT 0;
");
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[NewsletterSubscribers]', N'U') IS NOT NULL
    DROP TABLE [NewsletterSubscribers];
");
            // SavedOpportunities and CompanyTeamMembers are not recreated on rollback.
        }
    }
}
