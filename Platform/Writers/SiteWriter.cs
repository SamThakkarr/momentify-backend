using momentify_backend.Data;
using momentify_backend.Platform.Database.Models;

namespace momentify_backend.Platform.Writers;

public interface ISiteWriter
{
    void Add(Site site);
    void Update(Site site);
}

public class SiteWriter : ISiteWriter
{
    private readonly AppDbContext _context;

    public SiteWriter(AppDbContext context)
    {
        _context = context;
    }

    public void Add(Site site)
    {
        _context.Sites.Add(site);
    }

    public void Update(Site site)
    {
        _context.Sites.Update(site);
    }
}

