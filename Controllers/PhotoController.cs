using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using momentify_backend.Services;
using momentify_backend.Services.Photo;

namespace momentify_backend.Controllers;

[ApiController]
[Route("api/photo")]
[Authorize]
public class PhotoController : ControllerBase
{
    private readonly IPhotoService _photoService;

    public PhotoController(
        IPhotoService photoService)
    {
        _photoService = photoService;
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Upload(
        IFormFile image,
        [FromForm] int? photoId = null,
        [FromForm] int displayOrder = -1,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var response = await _photoService.UploadAsync(
            userId,
            photoId,
            image,
            displayOrder,
            cancellationToken);

        if (photoId is > 0)
            return Ok(response);

        return Created(
            response.Url,
            response);
    }

    [HttpGet("{photoId:int}", Name = "GetPhoto")]
    public async Task<IActionResult> Get(
        int siteId,
        int photoId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var result = await _photoService.GetAsync(
            siteId,
            userId,
            photoId,
            cancellationToken);

        if (result == null)
            return NotFound();

        return File(
            result.Value.Stream,
            result.Value.ContentType);
    }

    [AllowAnonymous]
    [HttpGet("{photoId:int}/public")]
    public async Task<IActionResult> PublicGet(
        int siteId,
        int photoId,
        CancellationToken cancellationToken = default)
    {
        var result = await _photoService.PublicGetAsync(
            siteId,
            photoId,
            cancellationToken);

        if (result == null)
            return NotFound();

        return File(
            result.Value.Stream,
            result.Value.ContentType);
    }

    private bool TryGetUserId(out int userId)
    {
        var userIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);
        userId = 0;
        if (userIdClaim == null ||
            !int.TryParse(userIdClaim, out userId))
            return false;

        return true;
    }
}