using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GradGateway.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDirectJobOfferFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = 'IX_Applications_OpportunityId_StudentProfileId'
                      AND object_id = OBJECT_ID(N'[dbo].[Applications]')
                )
                BEGIN
                    DROP INDEX [IX_Applications_OpportunityId_StudentProfileId] ON [dbo].[Applications];
                END
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH('Applications', 'OpportunityId') IS NOT NULL
                BEGIN
                    ALTER TABLE [Applications] ALTER COLUMN [OpportunityId] UNIQUEIDENTIFIER NULL;
                END
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH('Applications', 'CompanyProfileId') IS NULL
                BEGIN
                    ALTER TABLE [Applications] ADD [CompanyProfileId] UNIQUEIDENTIFIER NULL;
                END
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH('Applications', 'JobTitle') IS NULL
                BEGIN
                    ALTER TABLE [Applications] ADD [JobTitle] NVARCHAR(200) NULL;
                END
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH('Applications', 'JobType') IS NULL
                BEGIN
                    ALTER TABLE [Applications] ADD [JobType] NVARCHAR(50) NULL;
                END
                """);

            migrationBuilder.Sql("""
                IF COL_LENGTH('Applications', 'Compensation') IS NULL
                BEGIN
                    ALTER TABLE [Applications] ADD [Compensation] NVARCHAR(100) NULL;
                END
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (
                    SELECT 1 FROM sys.foreign_keys
                    WHERE name = 'FK_Applications_CompanyProfiles_CompanyProfileId'
                )
                AND COL_LENGTH('Applications', 'CompanyProfileId') IS NOT NULL
                BEGIN
                    ALTER TABLE [Applications]
                    ADD CONSTRAINT [FK_Applications_CompanyProfiles_CompanyProfileId]
                    FOREIGN KEY ([CompanyProfileId]) REFERENCES [CompanyProfiles]([Id]);
                END
                """);

            migrationBuilder.Sql("""
                UPDATE a
                SET a.CompanyProfileId = o.CompanyProfileId
                FROM [Applications] a
                INNER JOIN [Opportunities] o ON a.OpportunityId = o.Id
                WHERE a.CompanyProfileId IS NULL AND a.OpportunityId IS NOT NULL;
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = 'IX_Applications_OpportunityId_StudentProfileId'
                      AND object_id = OBJECT_ID(N'[dbo].[Applications]')
                )
                BEGIN
                    CREATE UNIQUE INDEX [IX_Applications_OpportunityId_StudentProfileId]
                    ON [Applications] ([OpportunityId], [StudentProfileId])
                    WHERE [OpportunityId] IS NOT NULL;
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = 'IX_Applications_OpportunityId_StudentProfileId'
                      AND object_id = OBJECT_ID(N'[dbo].[Applications]')
                )
                BEGIN
                    DROP INDEX [IX_Applications_OpportunityId_StudentProfileId] ON [dbo].[Applications];
                END

                CREATE UNIQUE INDEX [IX_Applications_OpportunityId_StudentProfileId]
                ON [Applications] ([OpportunityId], [StudentProfileId]);
                """);
        }
    }
}
