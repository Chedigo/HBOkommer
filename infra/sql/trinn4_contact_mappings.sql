IF OBJECT_ID('dbo.ContactMappings', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ContactMappings
    (
        Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        SubjectRef NVARCHAR(200) NOT NULL,
        PhoneNumberE164 NVARCHAR(20) NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_ContactMappings_IsActive DEFAULT (1),
        Source NVARCHAR(50) NOT NULL CONSTRAINT DF_ContactMappings_Source DEFAULT ('PILOT_MAPPING'),
        UpdatedAtUtc DATETIMEOFFSET NOT NULL CONSTRAINT DF_ContactMappings_UpdatedAtUtc DEFAULT (SYSUTCDATETIME())
    );

    CREATE UNIQUE INDEX UX_ContactMappings_SubjectRef ON dbo.ContactMappings(SubjectRef);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.ContactMappings WHERE SubjectRef = 'patient-123')
BEGIN
    INSERT INTO dbo.ContactMappings (SubjectRef, PhoneNumberE164)
    VALUES ('patient-123', '+4799999999');
END
GO
