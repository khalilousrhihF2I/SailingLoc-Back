CREATE FUNCTION dbo.GetUserFullName(@UserId UNIQUEIDENTIFIER)
RETURNS NVARCHAR(205)
AS
BEGIN
    DECLARE @full NVARCHAR(205);
    SELECT @full =
        LTRIM(RTRIM(
            COALESCE(NULLIF(FirstName, N''), N'') + N' ' +
            COALESCE(NULLIF(LastName,  N''), N'')
        ))
    FROM dbo.AspNetUsers
    WHERE Id = @UserId;

    RETURN @full;
END