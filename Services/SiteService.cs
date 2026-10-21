using momentify_backend.Commands.CreateSite;
using momentify_backend.Commands.PublishSite;
using momentify_backend.DTOs;
using momentify_backend.Queries.GetSiteBySlug;
using momentify_backend.Queries.GetUserSites;

namespace momentify_backend.Services;

public interface ISiteService
{
    Task<CreateSiteResponse> CreateSiteAsync(
        CreateSiteRequest request);

    Task<List<UserSiteDto>> GetSitesByUserIdAsync(
        int userId);

    Task<UserSiteDto?> GetSiteBySlugAsync(
        string publicSlug);

    Task PublishSiteAsync(int siteId);
}

public class SiteService : ISiteService
{
    private readonly ICreateSiteCommandHandler _createSiteHandler;
    private readonly IGetUserSitesQueryHandler _getUserSitesHandler;
    private readonly IGetSiteBySlugQueryHandler _getSiteBySlugHandler;
    private readonly IPublishSiteCommandHandler _publishSiteHandler;

    public SiteService(
        ICreateSiteCommandHandler createSiteHandler,
        IGetUserSitesQueryHandler getUserSitesHandler,
        IGetSiteBySlugQueryHandler getSiteBySlugHandler,
        IPublishSiteCommandHandler publishSiteHandler)
    {
        _createSiteHandler = createSiteHandler;
        _getUserSitesHandler = getUserSitesHandler;
        _getSiteBySlugHandler = getSiteBySlugHandler;
        _publishSiteHandler = publishSiteHandler;
    }

    public async Task<CreateSiteResponse> CreateSiteAsync(
        CreateSiteRequest request)
    {
        return await _createSiteHandler.HandleAsync(request);
    }

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

    public async Task PublishSiteAsync(int siteId)
    {
        await _publishSiteHandler.HandleAsync(siteId);
    }
}
