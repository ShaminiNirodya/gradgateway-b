using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GradGateway.Data.Migrations;

/// <inheritdoc />
public partial class AddOpportunityInterviewPlan : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF OBJECT_ID(N'[OpportunityInterviewPlans]', N'U') IS NULL
            BEGIN
                CREATE TABLE [OpportunityInterviewPlans] (
                    [Id] uniqueidentifier NOT NULL,
                    [OpportunityId] uniqueidentifier NOT NULL,
                    [TentativeDatesJson] nvarchar(max) NOT NULL,
                    [DurationMinutes] int NOT NULL,
                    [Mode] int NOT NULL,
                    [MeetingLink] nvarchar(max) NULL,
                    [Location] nvarchar(max) NULL,
                    [Notes] nvarchar(max) NULL,
                    [CreatedAt] datetime2 NOT NULL,
                    [UpdatedAt] datetime2 NOT NULL,
                    CONSTRAINT [PK_OpportunityInterviewPlans] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_OpportunityInterviewPlans_Opportunities_OpportunityId]
                        FOREIGN KEY ([OpportunityId]) REFERENCES [Opportunities] ([Id]) ON DELETE CASCADE
                );
                CREATE UNIQUE INDEX [IX_OpportunityInterviewPlans_OpportunityId]
                    ON [OpportunityInterviewPlans] ([OpportunityId]);
            END
            """);

        migrationBuilder.Sql("""
            IF COL_LENGTH('Applications', 'InterviewPlanNotifiedAt') IS NULL
            BEGIN
                ALTER TABLE [Applications] ADD [InterviewPlanNotifiedAt] datetime2 NULL;
            END
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF COL_LENGTH('Applications', 'InterviewPlanNotifiedAt') IS NOT NULL
            BEGIN
                ALTER TABLE [Applications] DROP COLUMN [InterviewPlanNotifiedAt];
            END
            """);

        migrationBuilder.Sql("""
            IF OBJECT_ID(N'[OpportunityInterviewPlans]', N'U') IS NOT NULL
            BEGIN
                DROP TABLE [OpportunityInterviewPlans];
            END
            """);
    }
}
