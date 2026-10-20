using momentify_backend.DTOs;

namespace momentify_backend.Interfaces;

public interface ISiteService
{
    Task<IEnumerable<UserSiteDto>> GetUserSitesAsync(int userId);

    Task<CreateSiteResponse> CreateSiteAsync(CreateSiteRequest request);
}