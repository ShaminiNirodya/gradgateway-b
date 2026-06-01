-- Add Availability column to StudentProfiles table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[StudentProfiles]') AND name = 'Availability')
BEGIN
    ALTER TABLE [dbo].[StudentProfiles] 
    ADD [Availability] NVARCHAR(50) NOT NULL DEFAULT 'Available Now';
    
    PRINT 'Availability column added successfully';
END
ELSE
BEGIN
    PRINT 'Availability column already exists';
END
GO
