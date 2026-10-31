using momentify_backend.Data;

namespace momentify_backend.Platform.Interfaces;

public interface ITemplate
{
    int SiteId { get; set; }

    Task SaveAsync(AppDbContext context);
}
