using momentify_backend.Commands.CreateSite;
using momentify_backend.Commands.PublishSite;
using momentify_backend.Commands.UpdateSite;
using momentify_backend.Platform.Queries.GetSiteBySlug;
using momentify_backend.Platform.Queries.GetUserSites;
using momentify_backend.Services.Site.Contracts;

namespace momentify_backend.Services.Site;

public interface ISiteService
{
    Task<CreateSiteResponse> CreateSiteAsync(
        CreateSiteRequest request,
        int userId);

    Task<List<UserSiteDto>> GetSitesByUserIdAsync(
        int userId);

    Task<UserSiteDto?> GetSiteBySlugAsync(
        string publicSlug);

    Task UpdateSiteAsync(
        int siteId,
        int userId,
        CreateSiteRequest request);

    Task PublishSiteAsync(int siteId, int userId);
}

public class SiteService : ISiteService
{
    private readonly ICreateSiteCommandHandler _createSiteHandler;
    private readonly IGetUserSitesQueryHandler _getUserSitesHandler;
    private readonly IGetSiteBySlugQueryHandler _getSiteBySlugHandler;
    private readonly IUpdateSiteCommandHandler _updateSiteHandler;
    private readonly IPublishSiteCommandHandler _publishSiteHandler;

    public SiteService(
        ICreateSiteCommandHandler createSiteHandler,
        IGetUserSitesQueryHandler getUserSitesHandler,
        IGetSiteBySlugQueryHandler getSiteBySlugHandler,
        IUpdateSiteCommandHandler updateSiteHandler,
        IPublishSiteCommandHandler publishSiteHandler)
    {
        _createSiteHandler = createSiteHandler;
        _getUserSitesHandler = getUserSitesHandler;
        _getSiteBySlugHandler = getSiteBySlugHandler;
        _updateSiteHandler = updateSiteHandler;
        _publishSiteHandler = publishSiteHandler;
    }

    public async Task<CreateSiteResponse> CreateSiteAsync(
        CreateSiteRequest request,
        int userId) =>
        await _createSiteHandler.HandleAsync(request, userId);

    public async Task<List<UserSiteDto>> GetSitesByUserIdAsync(
        int userId)
    {
        return await _getUserSitesHandler.HandleAsync(userId);
    }

    public async Task<UserSiteDto?> GetSiteBySlugAsync(
        string publicSlug)
    {
        return await _getSiteBySlugHandler.HandleAsync(publicSlug);
    }

    public async Task UpdateSiteAsync(
        int siteId,
        int userId,
        CreateSiteRequest request)
    {
        await _updateSiteHandler.HandleAsync(siteId, userId, request);
    }

    public async Task PublishSiteAsync(int siteId, int userId)
    {
        await _publishSiteHandler.HandleAsync(siteId, userId);
    }
}
