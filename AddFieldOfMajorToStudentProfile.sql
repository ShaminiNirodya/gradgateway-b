-- Add FieldOfMajor to StudentProfiles (run in SSMS if not using EF migrate)
USE [GradGatewayDb];
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[StudentProfiles]') AND name = 'FieldOfMajor'
)
BEGIN
    ALTER TABLE [dbo].[StudentProfiles]
    ADD [FieldOfMajor] NVARCHAR(120) NOT NULL CONSTRAINT DF_StudentProfiles_FieldOfMajor DEFAULT '';
END
GO
