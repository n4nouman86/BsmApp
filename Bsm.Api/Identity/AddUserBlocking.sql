SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF COL_LENGTH('dbo.AspNetUsers', 'IsBlocked') IS NULL
    ALTER TABLE dbo.AspNetUsers ADD IsBlocked bit NOT NULL
        CONSTRAINT DF_AspNetUsers_IsBlocked DEFAULT (0);

IF COL_LENGTH('dbo.AspNetUsers', 'TokenVersion') IS NULL
    ALTER TABLE dbo.AspNetUsers ADD TokenVersion int NOT NULL
        CONSTRAINT DF_AspNetUsers_TokenVersion DEFAULT (0);

COMMIT TRANSACTION;
