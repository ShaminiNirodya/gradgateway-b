-- Links notifications to applications, conversations, and students for deep-link highlighting

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Notifications') AND name = N'RelatedApplicationId'
)
BEGIN
    ALTER TABLE dbo.Notifications ADD RelatedApplicationId uniqueidentifier NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Notifications') AND name = N'RelatedConversationId'
)
BEGIN
    ALTER TABLE dbo.Notifications ADD RelatedConversationId uniqueidentifier NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Notifications') AND name = N'RelatedStudentProfileId'
)
BEGIN
    ALTER TABLE dbo.Notifications ADD RelatedStudentProfileId uniqueidentifier NULL;
END
GO
