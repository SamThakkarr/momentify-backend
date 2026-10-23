using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using momentify_backend.Services;

namespace momentify_backend.Controllers;

[ApiController]
[Route("api/Site/{siteId:int}/photos")]
[Authorize]
public class PhotoController : ControllerBase
{
    private readonly IPhotoService _photoService;

    public PhotoController(IPhotoService photoService)
    {
        _photoService = photoService;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Upload(
        int siteId,
        IFormFile image,
        [FromForm] int displayOrder = -1,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();

        var response = await _photoService.UploadAsync(
            siteId,
            userId,
            image,
            displayOrder,
            cancellationToken);

        return Created(
            response.Url,
            response);
    }

    [HttpPut("{photoId:int}")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Update(
        int siteId,
        int photoId,
        IFormFile image,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();

        var response = await _photoService.UpdateAsync(
            siteId,
            userId,
            photoId,
            image,
            cancellationToken);

        return Ok(response);
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (userIdClaim == null ||
            !int.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException();
        }

        return userId;
    }
}
