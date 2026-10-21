using momentify_backend.Data;
using momentify_backend.Readers;
using momentify_backend.Writers;

namespace momentify_backend.Commands.PublishSite;

public interface IPublishSiteCommandHandler
{
    Task HandleAsync(int siteId);
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

    public async Task HandleAsync(int siteId)
    {
        var site = await _siteReader.GetByIdAsync(siteId);

        if (site == null)
            throw new InvalidOperationException("Site not found.");

        site.IsPublished = true;
        site.PublishedAt = DateTime.UtcNow;

        _siteWriter.Update(site);

        await _context.SaveChangesAsync();
    }
}
