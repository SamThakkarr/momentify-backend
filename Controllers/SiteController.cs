using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using momentify_backend.Services;
using momentify_backend.Services.Site;
using momentify_backend.Services.Site.Contracts;

namespace momentify_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SiteController : ControllerBase
{
    private readonly ISiteService _siteService;

    public SiteController(ISiteService siteService)
    {
        _siteService = siteService;
    }

    [HttpGet("user")]
    public async Task<IActionResult> GetUserSites(
        [FromQuery] int? siteId)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var sites =
            await _siteService.GetSitesByUserIdAsync(
                userId,
                siteId);

        return Ok(sites);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSite(
        CreateSiteRequest request)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var response =
            await _siteService.CreateSiteAsync(
                request,
                userId);

        return Ok(response);
    }

    [HttpPut("{siteId}")]
    public async Task<IActionResult> UpdateSite(
        int siteId,
        CreateSiteRequest request)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        await _siteService.UpdateSiteAsync(
            siteId,
            userId,
            request);

        return Ok();
    }

    [HttpPost("{siteId}/publish")]
    public async Task<IActionResult> PublishSite(int siteId)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        await _siteService.PublishSiteAsync(
            siteId,
            userId);

        return Ok();
    }

    [AllowAnonymous]
    [HttpGet("s/{publicSlug}")]
    public async Task<IActionResult> GetPublicSite(
        string publicSlug)
    {
        var site =
            await _siteService.GetSiteBySlugAsync(publicSlug);

        if (site == null)
            return NotFound();

        return Ok(site);
    }

    private bool TryGetUserId(out int userId)
    {
        userId = 0;

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userIdClaim == null ||
            !int.TryParse(userIdClaim, out userId))
            return false;

        return true;
    }
}