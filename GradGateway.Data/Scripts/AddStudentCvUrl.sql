-- Manual apply for migration 20260604120000_AddStudentCvUrl
-- Run in SSMS / Azure Data Studio against GradGatewayDb

IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.StudentProfiles')
      AND name = N'CvUrl'
)
BEGIN
    ALTER TABLE dbo.StudentProfiles
    ADD CvUrl nvarchar(max) NULL;
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM dbo.__EFMigrationsHistory
    WHERE MigrationId = N'20260604120000_AddStudentCvUrl'
)
BEGIN
    INSERT INTO dbo.__EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES (N'20260604120000_AddStudentCvUrl', N'9.0.0');
END
GO
