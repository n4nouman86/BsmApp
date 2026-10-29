SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF EXISTS (
    SELECT 1 FROM dbo.AspNetUsers WITH (TABLOCKX, HOLDLOCK)
    WHERE DATALENGTH(FirstName) > 100 OR DATALENGTH(LastName) > 100
)
BEGIN
    ;THROW 50001, 'User names exceed 50 characters. Resolve these values before applying this upgrade.', 1;
END;

ALTER TABLE dbo.AspNetUsers ALTER COLUMN FirstName nvarchar(50) NOT NULL;
ALTER TABLE dbo.AspNetUsers ALTER COLUMN LastName nvarchar(50) NOT NULL;

COMMIT TRANSACTION;
