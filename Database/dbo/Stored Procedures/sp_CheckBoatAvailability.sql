
-- -----------------------------------------------------------------------------
-- Procédure pour vérifier la disponibilité d'un bateau sur une période
-- -----------------------------------------------------------------------------
CREATE PROCEDURE [dbo].[sp_CheckBoatAvailability]
    @BoatId INT,
    @StartDate DATE,
    @EndDate DATE,
    @ExcludeBookingId NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Vérifier que les dates sont valides
    IF @StartDate >= @EndDate
    BEGIN
        SELECT 0 AS IsAvailable, 'La date de fin doit être postérieure à la date de début' AS Message;
        RETURN;
    END
    
    -- Vérifier que les dates ne sont pas dans le passé
    IF @StartDate < CAST(GETDATE() AS DATE)
    BEGIN
        SELECT 0 AS IsAvailable, 'Les dates ne peuvent pas être dans le passé' AS Message;
        RETURN;
    END
    
    -- Vérifier les réservations existantes
    IF EXISTS (
        SELECT 1 FROM Bookings
        WHERE BoatId = @BoatId
        AND Status NOT IN ('cancelled')
        AND (@ExcludeBookingId IS NULL OR Id != @ExcludeBookingId)
        AND (
            -- La nouvelle réservation chevauche une réservation existante
            (@StartDate BETWEEN StartDate AND EndDate)
            OR (@EndDate BETWEEN StartDate AND EndDate)
            OR (StartDate BETWEEN @StartDate AND @EndDate)
            OR (EndDate BETWEEN @StartDate AND @EndDate)
        )
    )
    BEGIN
        SELECT 0 AS IsAvailable, 'Ce bateau est déjà réservé sur cette période' AS Message;
        RETURN;
    END
    
    -- Vérifier les périodes d'indisponibilité manuelles
    IF EXISTS (
        SELECT 1 FROM BoatAvailability
        WHERE BoatId = @BoatId
        AND IsAvailable = 0
        AND (
            (@StartDate BETWEEN StartDate AND EndDate)
            OR (@EndDate BETWEEN StartDate AND EndDate)
            OR (StartDate BETWEEN @StartDate AND @EndDate)
            OR (EndDate BETWEEN @StartDate AND @EndDate)
        )
    )
    BEGIN
        SELECT 0 AS IsAvailable, 'Ce bateau n''est pas disponible sur cette période' AS Message;
        RETURN;
    END
    
    -- Le bateau est disponible
    SELECT 1 AS IsAvailable, 'Le bateau est disponible' AS Message;
END
GO