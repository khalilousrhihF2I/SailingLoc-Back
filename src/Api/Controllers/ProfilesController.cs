using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;
[ApiController]
[Route("api/v1/profiles")]
public class ProfilesController : ControllerBase {
  [HttpPost, Authorize]
  public IActionResult Create() => StatusCode(501, new { message = "Profiles API placeholder" });
}
