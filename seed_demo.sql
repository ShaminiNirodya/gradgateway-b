SET NOCOUNT ON;

DECLARE @now DATETIME2 = SYSUTCDATETIME();

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = 'amaya.silva@uoc.lk')
BEGIN
    DECLARE @u1 UNIQUEIDENTIFIER = NEWID();
    INSERT INTO dbo.Users (Id, FirebaseUid, Email, Role, CreatedAt)
    VALUES (@u1, 'runtime-student-amaya', 'amaya.silva@uoc.lk', 1, DATEADD(DAY, -120, @now));

    INSERT INTO dbo.StudentProfiles (Id, UserId, FullName, Phone, PhotoDataUrl, University, StudentId, Degree, GradYear, Gpa, CertificationsJson, AwardsJson, CreatedAt, UpdatedAt)
    VALUES (NEWID(), @u1, 'Amaya Silva', '+94771112233', NULL, 'University of Colombo', 'UOC2023004', 'BSc (Hons) in Computer Science', 2026, 3.86,
            N'["Meta Front-End Developer - Coursera (2025)","Advanced React - Udemy (2025)"]',
            N'["Dean''s List - 2024", "Hackathon Winner - UoC InnovateX 2025"]',
            DATEADD(DAY, -120, @now), DATEADD(DAY, -10, @now));
END

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = 'dineth.fernando@uom.lk')
BEGIN
    DECLARE @u2 UNIQUEIDENTIFIER = NEWID();
    INSERT INTO dbo.Users (Id, FirebaseUid, Email, Role, CreatedAt)
    VALUES (@u2, 'runtime-student-dineth', 'dineth.fernando@uom.lk', 1, DATEADD(DAY, -115, @now));

    INSERT INTO dbo.StudentProfiles (Id, UserId, FullName, Phone, PhotoDataUrl, University, StudentId, Degree, GradYear, Gpa, CertificationsJson, AwardsJson, CreatedAt, UpdatedAt)
    VALUES (NEWID(), @u2, 'Dineth Fernando', '+94772223344', NULL, 'University of Moratuwa', 'UOM2022120', 'BSc Engineering', 2025, 3.91,
            N'["IBM Data Analyst Professional Certificate (2025)"]',
            N'["Best Final Year ML Project - UoM (2025)"]',
            DATEADD(DAY, -115, @now), DATEADD(DAY, -8, @now));
END

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = 'nethmi.wijesinghe@pdn.ac.lk')
BEGIN
    DECLARE @u3 UNIQUEIDENTIFIER = NEWID();
    INSERT INTO dbo.Users (Id, FirebaseUid, Email, Role, CreatedAt)
    VALUES (@u3, 'runtime-student-nethmi', 'nethmi.wijesinghe@pdn.ac.lk', 1, DATEADD(DAY, -110, @now));

    INSERT INTO dbo.StudentProfiles (Id, UserId, FullName, Phone, PhotoDataUrl, University, StudentId, Degree, GradYear, Gpa, CertificationsJson, AwardsJson, CreatedAt, UpdatedAt)
    VALUES (NEWID(), @u3, 'Nethmi Wijesinghe', '+94773334455', NULL, 'University of Peradeniya', 'PDN2023345', 'BSc in Information Technology', 2027, 3.74,
            N'["Google UX Design Certificate (2024)"]',
            N'["Inter-University App Challenge Finalist (2025)"]',
            DATEADD(DAY, -110, @now), DATEADD(DAY, -7, @now));
END

UPDATE dbo.StudentProfiles
SET CertificationsJson = ISNULL(CertificationsJson, N'["Responsive Web Design - freeCodeCamp (2024)"]'),
    AwardsJson = ISNULL(AwardsJson, N'["Dean''s List - Academic Excellence"]')
WHERE CertificationsJson IS NULL OR AwardsJson IS NULL;

DECLARE @company TABLE (rn INT IDENTITY(1,1), CompanyProfileId UNIQUEIDENTIFIER, CompanyName NVARCHAR(200));
INSERT INTO @company (CompanyProfileId, CompanyName)
SELECT TOP 10 Id, CompanyName FROM dbo.CompanyProfiles ORDER BY UpdatedAt DESC;

IF NOT EXISTS (SELECT 1 FROM dbo.Opportunities WHERE Title = 'Frontend Developer Intern - React')
INSERT INTO dbo.Opportunities (Id, CompanyProfileId, Title, Description, OpportunityType, WorkMode, Location, RequiredSkills, MonthlyStipendLkr, DeadlineAt, IsActive, CreatedAt, UpdatedAt)
SELECT NEWID(), c.CompanyProfileId, 'Frontend Developer Intern - React',
       'Work with product squads to build responsive UI modules for customer portals.',
       0, 1, 'Colombo', 'React, TypeScript, Tailwind CSS, REST APIs', 85000,
       DATEADD(DAY, 45, @now), 1, DATEADD(DAY, -12, @now), DATEADD(DAY, -5, @now)
FROM @company c WHERE c.rn = 1;

IF NOT EXISTS (SELECT 1 FROM dbo.Opportunities WHERE Title = 'Data Analytics Intern - BI')
INSERT INTO dbo.Opportunities (Id, CompanyProfileId, Title, Description, OpportunityType, WorkMode, Location, RequiredSkills, MonthlyStipendLkr, DeadlineAt, IsActive, CreatedAt, UpdatedAt)
SELECT NEWID(), c.CompanyProfileId, 'Data Analytics Intern - BI',
       'Analyze telecom and product datasets; create dashboards used by leadership teams.',
       0, 0, 'Battaramulla', 'Python, SQL, Power BI, Statistics', 95000,
       DATEADD(DAY, 50, @now), 1, DATEADD(DAY, -10, @now), DATEADD(DAY, -4, @now)
FROM @company c WHERE c.rn = 2;

IF NOT EXISTS (SELECT 1 FROM dbo.Opportunities WHERE Title = 'Associate QA Automation Engineer')
INSERT INTO dbo.Opportunities (Id, CompanyProfileId, Title, Description, OpportunityType, WorkMode, Location, RequiredSkills, MonthlyStipendLkr, DeadlineAt, IsActive, CreatedAt, UpdatedAt)
SELECT NEWID(), c.CompanyProfileId, 'Associate QA Automation Engineer',
       'Build scalable test automation suites for enterprise applications and APIs.',
       1, 2, 'Sri Lanka', 'Playwright, Selenium, C#, CI/CD', 165000,
       DATEADD(DAY, 60, @now), 1, DATEADD(DAY, -9, @now), DATEADD(DAY, -3, @now)
FROM @company c WHERE c.rn = 3;

IF NOT EXISTS (SELECT 1 FROM dbo.Opportunities WHERE Title = 'Cloud Support Intern - Azure')
INSERT INTO dbo.Opportunities (Id, CompanyProfileId, Title, Description, OpportunityType, WorkMode, Location, RequiredSkills, MonthlyStipendLkr, DeadlineAt, IsActive, CreatedAt, UpdatedAt)
SELECT NEWID(), c.CompanyProfileId, 'Cloud Support Intern - Azure',
       'Assist cloud team in monitoring, deployments, and cost optimization tasks.',
       0, 1, 'Colombo 07', 'Azure, Linux, Docker, Networking', 90000,
       DATEADD(DAY, 40, @now), 1, DATEADD(DAY, -8, @now), DATEADD(DAY, -2, @now)
FROM @company c WHERE c.rn = 4;

;WITH students AS (
    SELECT sp.Id AS StudentProfileId, sp.FullName, ROW_NUMBER() OVER (ORDER BY sp.UpdatedAt DESC) AS rn
    FROM dbo.StudentProfiles sp
), missing AS (
    SELECT s.*
    FROM students s
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Projects p WHERE p.StudentProfileId = s.StudentProfileId)
)
INSERT INTO dbo.Projects (Id, StudentProfileId, Title, Description, TechStack, RepositoryUrl, DemoUrl, IsPublic, CreatedAt, UpdatedAt)
SELECT NEWID(), m.StudentProfileId,
       CONCAT(m.FullName, ' Portfolio Project'),
       'Practical project showcasing internship-ready software engineering skills.',
       CASE WHEN m.rn % 2 = 0 THEN 'React, Node.js, PostgreSQL' ELSE 'ASP.NET Core, SQL Server, Next.js' END,
       'https://github.com/gradgateway/demo-student-project',
       NULL,
       1,
       DATEADD(DAY, -20, @now),
       DATEADD(DAY, -6, @now)
FROM missing m;

;WITH s AS (
    SELECT TOP 8 Id AS StudentProfileId, ROW_NUMBER() OVER (ORDER BY UpdatedAt DESC) AS s_rn
    FROM dbo.StudentProfiles
), o AS (
    SELECT TOP 8 Id AS OpportunityId, ROW_NUMBER() OVER (ORDER BY CreatedAt DESC) AS o_rn
    FROM dbo.Opportunities
), pairs AS (
    SELECT TOP 20
           s.StudentProfileId,
           o.OpportunityId,
           ROW_NUMBER() OVER (ORDER BY s.s_rn, o.o_rn) AS rn
    FROM s
    CROSS JOIN o
)
INSERT INTO dbo.Applications (Id, OpportunityId, StudentProfileId, CoverLetter, Status, AppliedAt, UpdatedAt)
SELECT NEWID(), p.OpportunityId, p.StudentProfileId,
       'I am highly interested in this role and can contribute from day one with strong project experience.',
       CASE WHEN p.rn % 4 = 0 THEN 1 WHEN p.rn % 7 = 0 THEN 2 ELSE 0 END,
       DATEADD(DAY, -(p.rn % 14 + 1), @now),
       DATEADD(DAY, -(p.rn % 7), @now)
FROM pairs p
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.Applications a
    WHERE a.OpportunityId = p.OpportunityId
      AND a.StudentProfileId = p.StudentProfileId
);

;WITH topApps AS (
    SELECT TOP 12
        a.OpportunityId,
        a.StudentProfileId,
        o.CompanyProfileId,
        ROW_NUMBER() OVER (ORDER BY a.UpdatedAt DESC) AS rn
    FROM dbo.Applications a
    JOIN dbo.Opportunities o ON o.Id = a.OpportunityId
    ORDER BY a.UpdatedAt DESC
)
INSERT INTO dbo.Conversations (Id, StudentProfileId, CompanyProfileId, OpportunityId, CreatedAt, LastMessageAt)
SELECT NEWID(), t.StudentProfileId, t.CompanyProfileId, t.OpportunityId,
       DATEADD(DAY, -3, @now), DATEADD(HOUR, -2, @now)
FROM topApps t
WHERE t.rn <= 8
  AND NOT EXISTS (
      SELECT 1
      FROM dbo.Conversations c
      WHERE c.StudentProfileId = t.StudentProfileId
        AND c.CompanyProfileId = t.CompanyProfileId
        AND ((c.OpportunityId = t.OpportunityId) OR (c.OpportunityId IS NULL AND t.OpportunityId IS NULL))
  );

;WITH conv AS (
    SELECT c.Id, sp.UserId AS StudentUserId, cp.UserId AS CompanyUserId
    FROM dbo.Conversations c
    JOIN dbo.StudentProfiles sp ON sp.Id = c.StudentProfileId
    JOIN dbo.CompanyProfiles cp ON cp.Id = c.CompanyProfileId
)
INSERT INTO dbo.Messages (Id, ConversationId, SenderUserId, Content, IsRead, SentAt)
SELECT NEWID(), conv.Id, conv.StudentUserId,
       'Hello, thank you for considering my application. Could you share the next steps?',
       1,
       DATEADD(HOUR, -5, @now)
FROM conv
WHERE NOT EXISTS (SELECT 1 FROM dbo.Messages m WHERE m.ConversationId = conv.Id);

;WITH conv AS (
    SELECT c.Id, sp.UserId AS StudentUserId, cp.UserId AS CompanyUserId
    FROM dbo.Conversations c
    JOIN dbo.StudentProfiles sp ON sp.Id = c.StudentProfileId
    JOIN dbo.CompanyProfiles cp ON cp.Id = c.CompanyProfileId
)
INSERT INTO dbo.Messages (Id, ConversationId, SenderUserId, Content, IsRead, SentAt)
SELECT NEWID(), conv.Id, conv.CompanyUserId,
       'Thanks for your message. We are reviewing profiles and will schedule interviews shortly.',
       0,
       DATEADD(HOUR, -2, @now)
FROM conv
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Messages m WHERE m.ConversationId = conv.Id AND m.SenderUserId = conv.CompanyUserId
);

INSERT INTO dbo.Notifications (Id, UserId, Type, Title, Body, IsRead, CreatedAt)
SELECT NEWID(), sp.UserId, 1, 'Application Update', 'Your recent application is under review by the hiring team.', 0, DATEADD(HOUR, -1, @now)
FROM dbo.StudentProfiles sp
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.Notifications n
    WHERE n.UserId = sp.UserId
      AND n.Title = 'Application Update'
      AND n.CreatedAt > DATEADD(DAY, -2, @now)
);

SELECT 'Users' AS [TableName], COUNT(*) AS [Rows] FROM dbo.Users UNION ALL
SELECT 'StudentProfiles', COUNT(*) FROM dbo.StudentProfiles UNION ALL
SELECT 'CompanyProfiles', COUNT(*) FROM dbo.CompanyProfiles UNION ALL
SELECT 'Opportunities', COUNT(*) FROM dbo.Opportunities UNION ALL
SELECT 'Applications', COUNT(*) FROM dbo.Applications UNION ALL
SELECT 'Conversations', COUNT(*) FROM dbo.Conversations UNION ALL
SELECT 'Messages', COUNT(*) FROM dbo.Messages UNION ALL
SELECT 'Notifications', COUNT(*) FROM dbo.Notifications UNION ALL
SELECT 'Projects', COUNT(*) FROM dbo.Projects UNION ALL
SELECT 'Skills', COUNT(*) FROM dbo.Skills UNION ALL
SELECT 'StudentSkills', COUNT(*) FROM dbo.StudentSkills UNION ALL
SELECT 'Interviews', COUNT(*) FROM dbo.Interviews UNION ALL
SELECT 'Documents', COUNT(*) FROM dbo.Documents
ORDER BY [TableName];