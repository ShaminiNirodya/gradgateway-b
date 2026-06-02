-- Migration script to update Application table for Job Offers feature
-- Run this in SQL Server Management Studio or sqlcmd

USE [GradGatewayDb];
GO

PRINT 'Starting Application schema migration...';
GO

-- 1. Make OpportunityId nullable (if not already)
IF EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Applications]') 
    AND name = 'OpportunityId' 
    AND is_nullable = 0
)
BEGIN
    ALTER TABLE [dbo].[Applications]
    ALTER COLUMN [OpportunityId] UNIQUEIDENTIFIER NULL;
    
    PRINT 'OpportunityId is now nullable';
END
ELSE
BEGIN
    PRINT 'OpportunityId is already nullable';
END
GO

-- 2. Add CompanyProfileId column (nullable)
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Applications]') 
    AND name = 'CompanyProfileId'
)
BEGIN
    ALTER TABLE [dbo].[Applications]
    ADD [CompanyProfileId] UNIQUEIDENTIFIER NULL;
    
    -- Add foreign key constraint
    ALTER TABLE [dbo].[Applications]
    ADD CONSTRAINT FK_Applications_CompanyProfiles_CompanyProfileId 
    FOREIGN KEY ([CompanyProfileId]) 
    REFERENCES [dbo].[CompanyProfiles]([Id]);
    
    PRINT 'CompanyProfileId column added';
END
ELSE
BEGIN
    PRINT 'CompanyProfileId column already exists';
END
GO

-- 3. Add JobTitle column (nullable)
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Applications]') 
    AND name = 'JobTitle'
)
BEGIN
    ALTER TABLE [dbo].[Applications]
    ADD [JobTitle] NVARCHAR(200) NULL;
    
    PRINT 'JobTitle column added';
END
ELSE
BEGIN
    PRINT 'JobTitle column already exists';
END
GO

-- 4. Add JobType column (nullable)
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Applications]') 
    AND name = 'JobType'
)
BEGIN
    ALTER TABLE [dbo].[Applications]
    ADD [JobType] NVARCHAR(50) NULL;
    
    PRINT 'JobType column added';
END
ELSE
BEGIN
    PRINT 'JobType column already exists';
END
GO

-- 5. Add Compensation column (nullable)
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Applications]') 
    AND name = 'Compensation'
)
BEGIN
    ALTER TABLE [dbo].[Applications]
    ADD [Compensation] NVARCHAR(100) NULL;
    
    PRINT 'Compensation column added';
END
ELSE
BEGIN
    PRINT 'Compensation column already exists';
END
GO

-- 6. Update existing applications to set CompanyProfileId from Opportunity
-- This ensures existing data remains consistent
UPDATE a
SET a.CompanyProfileId = o.CompanyProfileId
FROM [dbo].[Applications] a
INNER JOIN [dbo].[Opportunities] o ON a.OpportunityId = o.Id
WHERE a.CompanyProfileId IS NULL AND a.OpportunityId IS NOT NULL;

PRINT 'Updated existing applications with CompanyProfileId from Opportunities';
GO

PRINT 'Migration completed successfully!';
PRINT '';
PRINT 'Summary of changes:';
PRINT '- OpportunityId is now nullable';
PRINT '- Added CompanyProfileId (nullable, with FK constraint)';
PRINT '- Added JobTitle (nullable, NVARCHAR(200))';
PRINT '- Added JobType (nullable, NVARCHAR(50))';
PRINT '- Added Compensation (nullable, NVARCHAR(100))';
PRINT '- Updated existing applications with CompanyProfileId';
PRINT '';
PRINT 'Note: ApplicationStatus enum now includes OfferSent (value: 4)';
PRINT 'This is handled in the application code.';
GO
