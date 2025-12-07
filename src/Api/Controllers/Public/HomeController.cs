using System.Threading.Tasks;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Public
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [AllowAnonymous]
    public class HomeController : ControllerBase
    {
        private readonly IHomeService _homeService;

        public HomeController(IHomeService homeService)
        {
            _homeService = homeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetHomeData()
        {
            try
            {
                var data = await _homeService.GetHomeDataAsync();
                return Ok(new { topBoatTypes = data.TopBoatTypes, popularBoats = data.PopularBoats });
            }
            catch (System.Exception ex)
            {
                // Log if needed
                return Problem("An error occured while retrieving home data.");
            }
        }
    }
}
