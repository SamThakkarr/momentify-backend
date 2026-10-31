using momentify_backend.Data;
using momentify_backend.Platform.Readers;
using momentify_backend.Platform.Writers;

namespace momentify_backend.Commands.PublishSite;

public interface IPublishSiteCommandHandler
{
    Task HandleAsync(int siteId, int userId);
}

public class PublishSiteCommandHandler : IPublishSiteCommandHandler
{
    private readonly AppDbContext _context;
    private readonly ISiteReader _siteReader;
    private readonly ISiteWriter _siteWriter;

    public PublishSiteCommandHandler(
        AppDbContext context,
        ISiteReader siteReader,
        ISiteWriter siteWriter)
    {
        _context = context;
        _siteReader = siteReader;
        _siteWriter = siteWriter;
    }

    public async Task HandleAsync(int siteId, int userId)
    {
        var site = await _siteReader.GetByIdAsync(siteId);

        if (site == null)
            throw new KeyNotFoundException("Site not found.");

        if (site.UserId != userId)
            throw new UnauthorizedAccessException(
                "You do not have permission to publish this site.");

        site.IsPublished = true;
        site.PublishedAt = DateTime.UtcNow;

        _siteWriter.Update(site);

        await _context.SaveChangesAsync();
    }
}
