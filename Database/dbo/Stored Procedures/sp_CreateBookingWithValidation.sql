
-- -----------------------------------------------------------------------------
-- Procédure pour créer une réservation avec vérification de disponibilité
-- -----------------------------------------------------------------------------
CREATE PROCEDURE [dbo].[sp_CreateBookingWithValidation]
    @BookingId NVARCHAR(50),
    @BoatId INT,
    @RenterId NVARCHAR(450),
    @StartDate DATE,
    @EndDate DATE,
    @DailyPrice DECIMAL(10,2),
    @Subtotal DECIMAL(10,2),
    @ServiceFee DECIMAL(10,2),
    @TotalPrice DECIMAL(10,2),
    @RenterName NVARCHAR(256),
    @RenterEmail NVARCHAR(256),
    @RenterPhone NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Vérifier la disponibilité
        DECLARE @IsAvailable BIT;
        DECLARE @Message NVARCHAR(500);
        
        EXEC [dbo].[sp_CheckBoatAvailability] 
            @BoatId = @BoatId,
            @StartDate = @StartDate,
            @EndDate = @EndDate;
        
        -- Si disponible, créer la réservation
        INSERT INTO Bookings (
            Id, BoatId, RenterId, StartDate, EndDate,
            DailyPrice, Subtotal, ServiceFee, TotalPrice,
            Status, RenterName, RenterEmail, RenterPhone,
            PaymentStatus, CreatedAt
        )
        VALUES (
            @BookingId, @BoatId, @RenterId, @StartDate, @EndDate,
            @DailyPrice, @Subtotal, @ServiceFee, @TotalPrice,
            'pending', @RenterName, @RenterEmail, @RenterPhone,
            'pending', GETUTCDATE()
        );
        
        -- Retourner la réservation créée
        SELECT * FROM Bookings WHERE Id = @BookingId;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        
        -- Retourner l'erreur
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END
GO
