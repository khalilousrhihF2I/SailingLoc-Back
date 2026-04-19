using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Tests.Helpers;

/// <summary>
/// Sets up a fake authenticated user on a controller for unit tests.
/// </summary>
public static class FakeUserHelper
{
    public static void SetFakeUser(ControllerBase controller, string userId = "00000000-0000-0000-0000-000000000001")
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }
}
