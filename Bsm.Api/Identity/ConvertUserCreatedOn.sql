SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.AspNetUsers')
        AND name = 'CreatedOn'
        AND TYPE_NAME(system_type_id) = 'datetimeoffset'
)
BEGIN
    -- Normalize to UTC before dropping the offset to preserve each instant.
    UPDATE dbo.AspNetUsers WITH (TABLOCKX, HOLDLOCK)
    SET CreatedOn = SWITCHOFFSET(CreatedOn, '+00:00')
    WHERE DATEPART(TZOFFSET, CreatedOn) <> 0;

    ALTER TABLE dbo.AspNetUsers ALTER COLUMN CreatedOn datetime2(7) NOT NULL;
END;

COMMIT TRANSACTION;
