SET NOCOUNT ON;
DECLARE @now DATETIME2 = SYSUTCDATETIME();

;WITH c AS (
    SELECT Id, ROW_NUMBER() OVER (ORDER BY CreatedAt, Id) AS rn
    FROM dbo.CompanyProfiles
)
UPDATE cp
SET
    CompanyName = CASE c.rn
        WHEN 1 THEN 'TechNova Lanka (Pvt) Ltd'
        WHEN 2 THEN 'CloudAxis Solutions'
        WHEN 3 THEN 'InspireX Digital'
        WHEN 4 THEN 'DataForge Analytics'
        ELSE cp.CompanyName END,
    Industry = CASE c.rn
        WHEN 1 THEN 'Software & IT'
        WHEN 2 THEN 'Cloud Infrastructure'
        WHEN 3 THEN 'Product Engineering'
        WHEN 4 THEN 'Data & AI'
        ELSE cp.Industry END,
    Website = ISNULL(cp.Website,
        CASE c.rn
            WHEN 1 THEN 'https://www.technova.lk'
            WHEN 2 THEN 'https://www.cloudaxis.io'
            WHEN 3 THEN 'https://www.inspirex.digital'
            WHEN 4 THEN 'https://www.dataforge.ai'
            ELSE 'https://www.gradgateway.lk' END),
    RecruiterName = ISNULL(NULLIF(cp.RecruiterName,''),
        CASE c.rn
            WHEN 1 THEN 'Nadeesha Perera'
            WHEN 2 THEN 'Ishani Fernando'
            WHEN 3 THEN 'Tharindu Silva'
            WHEN 4 THEN 'Madhavi Jayasinghe'
            ELSE 'HR Team' END),
    Position = ISNULL(NULLIF(cp.Position,''), 'Talent Acquisition Lead'),
    UpdatedAt = @now
FROM dbo.CompanyProfiles cp
JOIN c ON c.Id = cp.Id;

;WITH company AS (
    SELECT Id, ROW_NUMBER() OVER (ORDER BY UpdatedAt DESC, Id) AS rn
    FROM dbo.CompanyProfiles
)
INSERT INTO dbo.Opportunities (Id, CompanyProfileId, Title, Description, OpportunityType, WorkMode, Location, RequiredSkills, MonthlyStipendLkr, DeadlineAt, IsActive, CreatedAt, UpdatedAt)
SELECT NEWID(), company.Id,
       v.Title,
       v.Description,
       v.OpportunityType,
       v.WorkMode,
       v.Location,
       v.RequiredSkills,
       v.MonthlyStipend,
       DATEADD(DAY, v.DeadlineDays, @now),
       1,
       DATEADD(DAY, -v.CreatedOffset, @now),
       DATEADD(DAY, -v.UpdatedOffset, @now)
FROM company
JOIN (VALUES
    (1, 'Associate Frontend Engineer', 'Build candidate and recruiter workflows with modern web stack.', 1, 1, 'Colombo', 'React, TypeScript, Tailwind CSS, REST APIs', 190000, 50, 12, 4),
    (2, 'Cloud Operations Intern', 'Support CI/CD pipelines, observability and infra automation tasks.', 0, 2, 'Sri Lanka', 'Azure, Docker, Linux, Networking', 95000, 45, 10, 3),
    (3, 'Data Engineering Intern', 'Develop ETL and reporting pipelines for hiring analytics.', 0, 1, 'Battaramulla', 'Python, SQL, Power BI, Data Modeling', 90000, 48, 9, 2),
    (4, 'QA Automation Engineer', 'Create robust automated test suites for platform modules.', 1, 1, 'Colombo 07', 'Playwright, Selenium, C#, CI/CD', 175000, 55, 8, 2)
) v(rn, Title, Description, OpportunityType, WorkMode, Location, RequiredSkills, MonthlyStipend, DeadlineDays, CreatedOffset, UpdatedOffset)
ON company.rn = v.rn
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Opportunities o
    WHERE o.CompanyProfileId = company.Id AND o.Title = v.Title
);

;WITH apps AS (
    SELECT TOP 6 Id, ROW_NUMBER() OVER (ORDER BY AppliedAt DESC) AS rn
    FROM dbo.Applications
    WHERE Status = 0
)
UPDATE a
SET Status = CASE
        WHEN apps.rn IN (1,2,3) THEN 1
        WHEN apps.rn = 4 THEN 3
        ELSE 2
    END,
    UpdatedAt = DATEADD(DAY, -1, @now)
FROM dbo.Applications a
JOIN apps ON apps.Id = a.Id;

SELECT 'CompanyProfiles' AS [TableName], COUNT(*) AS [Rows] FROM dbo.CompanyProfiles UNION ALL
SELECT 'Opportunities', COUNT(*) FROM dbo.Opportunities UNION ALL
SELECT 'Applications', COUNT(*) FROM dbo.Applications UNION ALL
SELECT 'Conversations', COUNT(*) FROM dbo.Conversations UNION ALL
SELECT 'Messages', COUNT(*) FROM dbo.Messages
ORDER BY [TableName];