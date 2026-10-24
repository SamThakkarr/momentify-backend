using momentify_backend.Data;
using momentify_backend.Models;

namespace momentify_backend.Writers;

public interface IPhotoWriter
{
    Task<Photo> CreateAsync(
        Photo photo,
        CancellationToken cancellationToken = default);

    Task SaveAsync(
        CancellationToken cancellationToken = default);
}

public class PhotoWriter : IPhotoWriter
{
    private readonly AppDbContext _context;

    public PhotoWriter(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Photo> CreateAsync(
        Photo photo,
        CancellationToken cancellationToken = default)
    {
        _context.Photos.Add(photo);

        await _context.SaveChangesAsync(
            cancellationToken);

        return photo;
    }

    public async Task SaveAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(
            cancellationToken);
    }
}

