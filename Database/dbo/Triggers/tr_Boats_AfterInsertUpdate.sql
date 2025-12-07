
-- Trigger pour mettre à jour le nombre de bateaux d'une destination
CREATE TRIGGER [dbo].[tr_Boats_AfterInsertUpdate]
ON [dbo].[Boats]
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Destinations affectées
    DECLARE @DestinationIds TABLE (DestinationId INT);
    
    INSERT INTO @DestinationIds
    SELECT DISTINCT DestinationId FROM inserted WHERE DestinationId IS NOT NULL
    UNION
    SELECT DISTINCT DestinationId FROM deleted WHERE DestinationId IS NOT NULL;
    
    -- Mettre à jour chaque destination
    DECLARE @DestId INT;
    DECLARE dest_cursor CURSOR FOR SELECT DestinationId FROM @DestinationIds;
    
    OPEN dest_cursor;
    FETCH NEXT FROM dest_cursor INTO @DestId;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        EXEC [dbo].[sp_UpdateDestinationBoatCount] @DestId;
        FETCH NEXT FROM dest_cursor INTO @DestId;
    END
    
    CLOSE dest_cursor;
    DEALLOCATE dest_cursor;
END
GO