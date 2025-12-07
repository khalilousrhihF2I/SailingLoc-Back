
-- Vue pour les détails complets des réservations
CREATE VIEW [dbo].[vw_BookingDetails] AS
SELECT 
    bk.Id AS BookingId,
    bk.Status,
    bk.StartDate,
    bk.EndDate,
    bk.Days,
    bk.TotalPrice,
    bk.ServiceFee,
    bk.CreatedAt,
    
    -- Bateau
    b.Id AS BoatId,
    b.Name AS BoatName,
    b.Type AS BoatType,
    b.Image AS BoatImage,
    b.Location AS BoatLocation,
    
    -- Propriétaire
    owner.Id AS OwnerId,
    LTRIM(RTRIM(ISNULL(owner.FirstName, '') + ' ' + ISNULL(owner.LastName, ''))) AS OwnerName ,
    owner.Email AS OwnerEmail,
    
    -- Locataire
    renter.Id AS RenterId,
    LTRIM(RTRIM(ISNULL(renter.FirstName, '') + ' ' + ISNULL(renter.LastName, ''))) AS RenterName ,
    renter.Email AS RenterEmail
FROM Bookings bk
INNER JOIN Boats b ON bk.BoatId = b.Id
INNER JOIN AspNetUsers owner ON b.OwnerId = owner.Id
INNER JOIN AspNetUsers renter ON bk.RenterId = renter.Id;
GO