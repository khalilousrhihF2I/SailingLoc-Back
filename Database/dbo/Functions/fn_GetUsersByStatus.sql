CREATE FUNCTION dbo.fn_GetUsersByStatus(@Status dbo.UserStatus)
RETURNS TABLE
AS
RETURN
(
    SELECT
        u.Id,
        u.Email,
        u.FirstName,
        u.LastName,
        u.Status,
        u.CreatedAt,
        u.UpdatedAt
    FROM dbo.AspNetUsers AS u WITH (NOLOCK)
    WHERE u.Status = @Status
);