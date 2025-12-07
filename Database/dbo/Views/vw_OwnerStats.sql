CREATE VIEW [dbo].[vw_OwnerStats]
AS
SELECT 
    u.Id AS OwnerId,
    LTRIM(RTRIM(ISNULL(u.FirstName, '') + ' ' + ISNULL(u.LastName, ''))) AS OwnerName,
    COUNT(DISTINCT b.Id) AS BoatCount,
    COUNT(DISTINCT bk.Id) AS BookingCount,
    ISNULL(SUM(CASE 
        WHEN bk.Status = 'completed' 
        THEN bk.TotalPrice - bk.ServiceFee 
        ELSE 0 
    END), 0) AS TotalRevenue,
    ISNULL(AVG(b.Rating), 0) AS AverageRating
FROM dbo.AspNetUsers u
LEFT JOIN dbo.Boats b ON u.Id = b.OwnerId
LEFT JOIN dbo.Bookings bk ON b.Id = bk.BoatId
WHERE u.UserType = 'owner'
GROUP BY 
    u.Id,
    LTRIM(RTRIM(ISNULL(u.FirstName, '') + ' ' + ISNULL(u.LastName, '')));
