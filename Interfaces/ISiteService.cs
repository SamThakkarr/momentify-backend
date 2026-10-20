using momentify_backend.Models;

namespace momentify_backend.Interfaces;

public interface ISiteService
{
    Task<IEnumerable<object>> GetUserSitesAsync(int userId);
}

