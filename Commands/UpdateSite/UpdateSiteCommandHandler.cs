using momentify_backend.Data;
using momentify_backend.Platform.Interfaces;
using momentify_backend.Platform.Readers;
using momentify_backend.Services.Site.Contracts;

namespace momentify_backend.Commands.UpdateSite;

public interface IUpdateSiteCommandHandler
{
    Task HandleAsync(
        int siteId,
        int userId,
        CreateSiteRequest request);
}

public class UpdateSiteCommandHandler : IUpdateSiteCommandHandler
{
    private readonly AppDbContext _context;
    private readonly ISiteReader _siteReader;
    private readonly ITemplateFactory _templateFactory;
    private readonly ITemplateReader _templateReader;

    public UpdateSiteCommandHandler(
        AppDbContext context,
        ISiteReader siteReader,
        ITemplateReader templateReader,
        ITemplateFactory templateFactory)
    {
        _context = context;
        _siteReader = siteReader;
        _templateReader = templateReader;
        _templateFactory = templateFactory;
    }

    public async Task HandleAsync(
        int siteId,
        int userId,
        CreateSiteRequest request)
    {
        var site = await _siteReader.GetByIdAsync(siteId);

        if (site == null)
            throw new KeyNotFoundException("Site not found.");

        if (site.UserId != userId)
            throw new UnauthorizedAccessException(
                "You do not have permission to update this site.");

        var template = await _templateReader.GetByIdAsync(
            (int)request.TemplateType);

        if (template == null)
            throw new InvalidOperationException(
                "Template not found.");

        if (site.TemplateId != (int)request.TemplateType)
            throw new InvalidOperationException(
                "The template type cannot be changed.");

        var jsonData = request.Data;

        var templateData = _templateFactory.Create(
            request.TemplateType,
            jsonData);

        templateData.SiteId = siteId;

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            await templateData.SaveAsync(_context);

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}