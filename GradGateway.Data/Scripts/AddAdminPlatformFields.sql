-- Run in SSMS if EF migrate is unavailable

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Users') AND name = 'IsActive')
BEGIN
    ALTER TABLE Users ADD IsActive bit NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT(1);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'CompanyProfiles') AND name = 'VerificationStatus')
BEGIN
    ALTER TABLE CompanyProfiles ADD VerificationStatus int NOT NULL CONSTRAINT DF_CompanyProfiles_VerificationStatus DEFAULT(1);
    ALTER TABLE CompanyProfiles ADD VerificationRejectionReason nvarchar(max) NULL;
    ALTER TABLE CompanyProfiles ADD VerifiedAt datetime2 NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PlatformSettings')
BEGIN
    CREATE TABLE PlatformSettings (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        AllowRegistration bit NOT NULL,
        RequireCompanyVerification bit NOT NULL,
        MaintenanceMode bit NOT NULL,
        UpdatedAt datetime2 NOT NULL
    );

    INSERT INTO PlatformSettings (Id, AllowRegistration, RequireCompanyVerification, MaintenanceMode, UpdatedAt)
    VALUES ('f0f0f0f0-1111-2222-3333-444444444444', 1, 1, 0, '2026-02-01T00:00:00Z');
END
GO

UPDATE CompanyProfiles
SET VerificationStatus = 1, VerifiedAt = COALESCE(VerifiedAt, '2026-02-01T00:00:00Z')
WHERE VerificationStatus = 0 OR VerifiedAt IS NULL;
GO

UPDATE Users SET IsActive = 1 WHERE IsActive IS NULL;
GO
