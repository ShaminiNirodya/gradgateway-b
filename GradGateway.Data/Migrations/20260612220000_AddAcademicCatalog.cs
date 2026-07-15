using GradGateway.Data.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GradGateway.Data.Migrations
{
    [DbContext(typeof(GradGatewayDbContext))]
    [Migration("20260612220000_AddAcademicCatalog")]
    public partial class AddAcademicCatalog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[CatalogUniversities]', N'U') IS NULL
BEGIN
    CREATE TABLE [CatalogUniversities] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [IsActive] bit NOT NULL CONSTRAINT DF_CatalogUniversities_IsActive DEFAULT 1,
        [SortOrder] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_CatalogUniversities] PRIMARY KEY ([Id])
    );
    CREATE UNIQUE INDEX [IX_CatalogUniversities_Name] ON [CatalogUniversities] ([Name]);
    CREATE INDEX [IX_CatalogUniversities_IsActive_SortOrder] ON [CatalogUniversities] ([IsActive], [SortOrder]);
END
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[CatalogDegrees]', N'U') IS NULL
BEGIN
    CREATE TABLE [CatalogDegrees] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [IsActive] bit NOT NULL CONSTRAINT DF_CatalogDegrees_IsActive DEFAULT 1,
        [SortOrder] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_CatalogDegrees] PRIMARY KEY ([Id])
    );
    CREATE UNIQUE INDEX [IX_CatalogDegrees_Name] ON [CatalogDegrees] ([Name]);
    CREATE INDEX [IX_CatalogDegrees_IsActive_SortOrder] ON [CatalogDegrees] ([IsActive], [SortOrder]);
END
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[CatalogUniversityDegrees]', N'U') IS NULL
BEGIN
    CREATE TABLE [CatalogUniversityDegrees] (
        [UniversityId] uniqueidentifier NOT NULL,
        [DegreeId] uniqueidentifier NOT NULL,
        [IsActive] bit NOT NULL CONSTRAINT DF_CatalogUniversityDegrees_IsActive DEFAULT 1,
        CONSTRAINT [PK_CatalogUniversityDegrees] PRIMARY KEY ([UniversityId], [DegreeId]),
        CONSTRAINT [FK_CatalogUniversityDegrees_CatalogUniversities_UniversityId]
            FOREIGN KEY ([UniversityId]) REFERENCES [CatalogUniversities] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CatalogUniversityDegrees_CatalogDegrees_DegreeId]
            FOREIGN KEY ([DegreeId]) REFERENCES [CatalogDegrees] ([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_CatalogUniversityDegrees_DegreeId] ON [CatalogUniversityDegrees] ([DegreeId]);
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[CatalogUniversityDegrees]', N'U') IS NOT NULL
    DROP TABLE [CatalogUniversityDegrees];

IF OBJECT_ID(N'[CatalogDegrees]', N'U') IS NOT NULL
    DROP TABLE [CatalogDegrees];

IF OBJECT_ID(N'[CatalogUniversities]', N'U') IS NOT NULL
    DROP TABLE [CatalogUniversities];
");
        }
    }
}
