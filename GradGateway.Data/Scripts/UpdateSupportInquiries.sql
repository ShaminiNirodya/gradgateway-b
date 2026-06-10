IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('SupportInquiries') AND name = 'SubmitterRole'
)
BEGIN
    ALTER TABLE SupportInquiries ADD SubmitterRole nvarchar(20) NULL;
END
GO

DELETE FROM SupportInquiries WHERE Email = 'test@example.com';
GO

UPDATE SupportInquiries
SET SubmitterRole = 'Student'
WHERE SubmitterRole IS NULL AND Message LIKE '%[Student%';
GO

UPDATE SupportInquiries
SET SubmitterRole = 'Company'
WHERE SubmitterRole IS NULL AND Message LIKE '%[Company%';
GO

UPDATE SupportInquiries
SET SubmitterRole = 'Public'
WHERE SubmitterRole IS NULL;
GO
