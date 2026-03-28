using Core.DTOs;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [EnableCors("Default")]
    public class SitemapController : ControllerBase
    {
        private readonly IBoatService _boatService;

        public SitemapController(IBoatService boatService)
        {
            _boatService = boatService;
        }

        /// <summary>
        /// Returns sitemap entries for all active boats (id, slug, updatedAt).
        /// The frontend or a build script can merge this with the static sitemap.
        /// </summary>
        [HttpGet("boats")]
        [AllowAnonymous]
        [ResponseCache(Duration = 3600)]
        public async Task<IActionResult> GetBoatSitemapEntries(CancellationToken ct)
        {
            var result = await _boatService.GetBoatsAsync(new BoatFilters { Page = 1, PageSize = 100 });
            var entries = result.Items.Select(b => new
            {
                b.Id,
                b.Slug,
                b.Name,
                LastModified = b.UpdatedAt ?? b.CreatedAt
            });
            return Ok(entries);
        }
    }
}
