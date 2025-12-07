CREATE VIEW [dbo].[vw_RenterStats]
AS
SELECT 
    u.Id AS RenterId,
    LTRIM(RTRIM(ISNULL(u.FirstName, '') + ' ' + ISNULL(u.LastName, ''))) AS RenterName,
    COUNT(DISTINCT bk.Id) AS BookingCount,
    ISNULL(SUM(bk.TotalPrice), 0) AS TotalSpent,
    COUNT(DISTINCT r.Id) AS ReviewCount
FROM dbo.AspNetUsers u
LEFT JOIN dbo.Bookings bk ON u.Id = bk.RenterId
LEFT JOIN dbo.Reviews r ON u.Id = r.UserId
WHERE u.UserType = 'renter'
GROUP BY 
    u.Id,
    LTRIM(RTRIM(ISNULL(u.FirstName, '') + ' ' + ISNULL(u.LastName, '')));
