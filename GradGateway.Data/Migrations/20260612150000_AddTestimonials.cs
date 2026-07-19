using GradGateway.Data.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GradGateway.Data.Migrations
{
    [DbContext(typeof(GradGatewayDbContext))]
    [Migration("20260612150000_AddTestimonials")]
    public partial class AddTestimonials : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[Testimonials]', N'U') IS NULL
BEGIN
    CREATE TABLE [Testimonials] (
        [Id] uniqueidentifier NOT NULL,
        [Quote] nvarchar(500) NOT NULL,
        [AuthorName] nvarchar(120) NOT NULL,
        [AuthorRole] nvarchar(120) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [SortOrder] int NOT NULL,
        [SubmitterEmail] nvarchar(320) NULL,
        [SubmitterRole] nvarchar(40) NULL,
        [SubmittedByUserId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [PublishedAt] datetime2 NULL,
        [ReviewedAt] datetime2 NULL,
        CONSTRAINT [PK_Testimonials] PRIMARY KEY ([Id])
    );
    CREATE INDEX [IX_Testimonials_Status] ON [Testimonials] ([Status]);
    CREATE INDEX [IX_Testimonials_SortOrder] ON [Testimonials] ([SortOrder]);
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [Testimonials] WHERE [Id] = 'f1f1f1f1-1111-2222-3333-444444444441')
    INSERT INTO [Testimonials] ([Id], [Quote], [AuthorName], [AuthorRole], [Status], [SortOrder], [CreatedAt], [PublishedAt], [ReviewedAt])
    VALUES (
        'f1f1f1f1-1111-2222-3333-444444444441',
        N'Having my projects and applications in one place made follow-ups with companies much easier.',
        N'Undergraduate, Colombo',
        N'Computer Science',
        N'Published',
        1,
        '2026-02-01T00:00:00.0000000',
        '2026-02-01T00:00:00.0000000',
        '2026-02-01T00:00:00.0000000'
    );

IF NOT EXISTS (SELECT 1 FROM [Testimonials] WHERE [Id] = 'f1f1f1f1-1111-2222-3333-444444444442')
    INSERT INTO [Testimonials] ([Id], [Quote], [AuthorName], [AuthorRole], [Status], [SortOrder], [CreatedAt], [PublishedAt], [ReviewedAt])
    VALUES (
        'f1f1f1f1-1111-2222-3333-444444444442',
        N'We shortlist faster because we see portfolios, CVs, and message history without switching tools.',
        N'Tech recruiter',
        N'Hiring partner',
        N'Published',
        2,
        '2026-02-01T00:00:00.0000000',
        '2026-02-01T00:00:00.0000000',
        '2026-02-01T00:00:00.0000000'
    );

IF NOT EXISTS (SELECT 1 FROM [Testimonials] WHERE [Id] = 'f1f1f1f1-1111-2222-3333-444444444443')
    INSERT INTO [Testimonials] ([Id], [Quote], [AuthorName], [AuthorRole], [Status], [SortOrder], [CreatedAt], [PublishedAt], [ReviewedAt])
    VALUES (
        'f1f1f1f1-1111-2222-3333-444444444443',
        N'The application pipeline and offer flow in messages saved our intern hiring season.',
        N'HR coordinator',
        N'Software company',
        N'Published',
        3,
        '2026-02-01T00:00:00.0000000',
        '2026-02-01T00:00:00.0000000',
        '2026-02-01T00:00:00.0000000'
    );
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[Testimonials]', N'U') IS NOT NULL
    DROP TABLE [Testimonials];
");
        }
    }
}
