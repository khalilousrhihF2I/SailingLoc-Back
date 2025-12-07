
-- Procédure pour mettre à jour la note moyenne d'un bateau
CREATE PROCEDURE [dbo].[sp_UpdateBoatRating]
    @BoatId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Boats
    SET 
        Rating = ISNULL((SELECT AVG(CAST(Rating AS DECIMAL(3,2))) FROM Reviews WHERE BoatId = @BoatId), 0),
        ReviewCount = ISNULL((SELECT COUNT(*) FROM Reviews WHERE BoatId = @BoatId), 0),
        UpdatedAt = GETUTCDATE()
    WHERE Id = @BoatId;
END
GO
