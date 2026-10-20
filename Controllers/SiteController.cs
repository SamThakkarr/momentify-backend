using Microsoft.AspNetCore.Mvc;
using momentify_backend.DTOs;
using momentify_backend.Interfaces;

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

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserSites(int userId)
    {
        var sites = await _siteService.GetUserSitesAsync(userId);

        return Ok(sites);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSite(CreateSiteRequest request)
    {
        try
        {
            var response = await _siteService.CreateSiteAsync(request);

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }
}
