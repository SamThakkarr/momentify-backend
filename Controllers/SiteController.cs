using Microsoft.AspNetCore.Mvc;
using momentify_backend.DTOs;
using momentify_backend.Services;

namespace momentify_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SiteController : ControllerBase
{
    private readonly ISiteService _siteService;

    public SiteController(ISiteService siteService)
    {
        _siteService = siteService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateSite(CreateSiteRequest request)
    {
        var result = await _siteService.CreateSiteAsync(request);

        return Ok(result);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserSites(int userId)
    {
        var sites = await _siteService.GetSitesByUserIdAsync(userId);

        return Ok(sites);
    }

    [HttpGet("s/{publicSlug}")]
    public async Task<IActionResult> GetPublicSite(string publicSlug)
    {
        var site = await _siteService.GetSiteBySlugAsync(publicSlug);

        if (site == null)
            return NotFound();

        return Ok(site);
    }

    [HttpPost("{siteId}/publish")]
    public async Task<IActionResult> PublishSite(int siteId)
    {
        await _siteService.PublishSiteAsync(siteId);

        return Ok();
    }
}
