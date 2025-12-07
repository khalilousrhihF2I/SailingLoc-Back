CREATE PROCEDURE dbo.GetUserWithRoles
  @UserId UNIQUEIDENTIFIER
AS
BEGIN
  SET NOCOUNT ON;
  SELECT u.Id, u.Email, u.FirstName, u.LastName, r.Name AS RoleName
  FROM dbo.AspNetUsers u
  LEFT JOIN dbo.AspNetUserRoles ur ON ur.UserId = u.Id
  LEFT JOIN dbo.AspNetRoles r ON r.Id = ur.RoleId
  WHERE u.Id = @UserId;
END