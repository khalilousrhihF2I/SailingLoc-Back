-- Procédure pour mettre à jour le nombre de bateaux d'une destination
CREATE PROCEDURE [dbo].[sp_UpdateDestinationBoatCount]
    @DestinationId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Destinations
    SET 
        BoatCount = ISNULL((SELECT COUNT(*) FROM Boats WHERE DestinationId = @DestinationId AND IsActive = 1), 0),
        UpdatedAt = GETUTCDATE()
    WHERE Id = @DestinationId;
END
GO