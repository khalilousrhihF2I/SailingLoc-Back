
-- -----------------------------------------------------------------------------
-- Procédure pour récupérer les dates indisponibles d'un bateau
-- -----------------------------------------------------------------------------
CREATE PROCEDURE [dbo].[sp_GetBoatUnavailableDates]
    @BoatId INT,
    @StartDate DATE = NULL,
    @EndDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Définir les dates par défaut si non fournies
    IF @StartDate IS NULL
        SET @StartDate = CAST(GETDATE() AS DATE);
    
    IF @EndDate IS NULL
        SET @EndDate = DATEADD(YEAR, 1, @StartDate);
    
    -- Récupérer toutes les périodes indisponibles
    SELECT 
        'booking' AS Type,
        Id AS ReferenceId,
        StartDate,
        EndDate,
        Status AS Reason,
        RenterName AS Details
    FROM Bookings
    WHERE BoatId = @BoatId
    AND Status NOT IN ('cancelled')
    AND EndDate >= @StartDate
    AND StartDate <= @EndDate
    
    UNION ALL
    
    SELECT 
        'unavailable' AS Type,
        CAST(Id AS NVARCHAR(50)) AS ReferenceId,
        StartDate,
        EndDate,
        Reason,
        NULL AS Details
    FROM BoatAvailability
    WHERE BoatId = @BoatId
    AND IsAvailable = 0
    AND EndDate >= @StartDate
    AND StartDate <= @EndDate
    
    ORDER BY StartDate;
END
GO