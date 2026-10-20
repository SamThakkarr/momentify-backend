using momentify_backend.Data;

namespace momentify_backend.Interfaces;

public interface ITemplate
{
    int SiteId { get; set; }

    Task SaveAsync(AppDbContext context);
}
