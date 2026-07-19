IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SupportInquiries')
BEGIN
    CREATE TABLE SupportInquiries (
        Id uniqueidentifier NOT NULL PRIMARY KEY,
        Name nvarchar(200) NOT NULL,
        Email nvarchar(320) NOT NULL,
        Phone nvarchar(40) NULL,
        InquiryType nvarchar(40) NOT NULL,
        Message nvarchar(max) NOT NULL,
        AttachmentName nvarchar(260) NULL,
        Status nvarchar(20) NOT NULL,
        CreatedAt datetime2 NOT NULL,
        ReviewedAt datetime2 NULL
    );

    CREATE INDEX IX_SupportInquiries_CreatedAt ON SupportInquiries (CreatedAt);
    CREATE INDEX IX_SupportInquiries_Status ON SupportInquiries (Status);
END
GO
