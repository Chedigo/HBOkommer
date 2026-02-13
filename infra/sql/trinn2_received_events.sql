-- Trinn 2: Idempotens (ReceivedEvents)
-- Database: HBOkommer
-- Formål: Samme eventId skal aldri kunne behandles to ganger.

SET NOCOUNT ON;
GO

USE [HBOkommer];
GO

IF OBJECT_ID(N'dbo.ReceivedEvents', N'U') IS NULL
BEGIN
    PRINT 'Creating table dbo.ReceivedEvents';

    CREATE TABLE dbo.ReceivedEvents
    (
        Id               BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ReceivedEvents PRIMARY KEY,
        EventId          NVARCHAR(128) NOT NULL,
        EventType        NVARCHAR(64)  NOT NULL,
        MunicipalityId   NVARCHAR(64)  NOT NULL,
        UnitId           NVARCHAR(128) NULL,
        SubjectRef       NVARCHAR(128) NULL,
        OccurredAtUtc    DATETIMEOFFSET(0) NOT NULL,
        ReceivedAtUtc    DATETIMEOFFSET(0) NOT NULL,
        SchemaVersion    NVARCHAR(16)  NULL
    );

    -- Idempotens: samme EventId kan aldri inn to ganger
    ALTER TABLE dbo.ReceivedEvents
        ADD CONSTRAINT UQ_ReceivedEvents_EventId UNIQUE (EventId);

    CREATE INDEX IX_ReceivedEvents_Municipality_ReceivedAt
        ON dbo.ReceivedEvents (MunicipalityId, ReceivedAtUtc);
END
ELSE
BEGIN
    PRINT 'Table exists: dbo.ReceivedEvents';
END
GO
