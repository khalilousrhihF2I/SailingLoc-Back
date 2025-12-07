-- Trigger pour mettre à jour automatiquement la note d'un bateau après l'ajout d'un avis
CREATE TRIGGER [dbo].[tr_Reviews_AfterInsert]
ON [dbo].[Reviews]
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @BoatId INT;
    SELECT @BoatId = BoatId FROM inserted;
    
    EXEC [dbo].[sp_UpdateBoatRating] @BoatId;
END
GO