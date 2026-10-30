using Microsoft.EntityFrameworkCore;
using momentify_backend.Data;
using momentify_backend.Models;

namespace momentify_backend.Readers;

public interface IPhotoReader
{
    Task<Photo?> GetByIdForUserAsync(
        int photoId,
        int userId,
        CancellationToken cancellationToken = default);

    Task<Photo?> GetByIdForUserAndSiteAsync(
        int photoId,
        int siteId,
        int userId,
        CancellationToken cancellationToken = default);

    Task<Photo?> GetPublishedPhotoAsync(
        int photoId,
        int siteId,
        CancellationToken cancellationToken = default);

    Task<bool> SiteBelongsToUserAsync(
        int siteId,
        int userId,
        CancellationToken cancellationToken = default);
}

public class PhotoReader : IPhotoReader
{
    private readonly AppDbContext _context;

    public PhotoReader(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Photo?> GetByIdForUserAsync(
        int photoId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Photos
            .FirstOrDefaultAsync(
                x => x.Id == photoId &&
                     x.UserId == userId,
                cancellationToken);
    }

    public async Task<Photo?> GetByIdForUserAndSiteAsync(
        int photoId,
        int siteId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await GetByIdForUserAsync(
            photoId,
            userId,
            cancellationToken);
    }

    public async Task<Photo?> GetPublishedPhotoAsync(
        int photoId,
        int siteId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Photos
            .FirstOrDefaultAsync(
                x => x.Id == photoId &&
                     _context.Sites.Any(site => site.Id == siteId &&
                                                site.IsPublished &&
                                                site.UserId == x.UserId),
                cancellationToken);
    }

    public async Task<bool> SiteBelongsToUserAsync(
        int siteId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Sites
            .AnyAsync(
                x => x.Id == siteId &&
                     x.UserId == userId,
                cancellationToken);
    }
}