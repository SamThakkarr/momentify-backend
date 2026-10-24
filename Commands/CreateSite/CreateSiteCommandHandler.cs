using momentify_backend.Data;
using momentify_backend.DTOs;
using momentify_backend.Interfaces;
using momentify_backend.Models;
using momentify_backend.Readers;
using momentify_backend.Writers;

namespace momentify_backend.Commands.CreateSite;

public interface ICreateSiteCommandHandler
{
    Task<CreateSiteResponse> HandleAsync(
        CreateSiteRequest request,
        int userId);
}

public class CreateSiteCommandHandler : ICreateSiteCommandHandler
{
    private readonly AppDbContext _context;
    private readonly ISiteWriter _siteWriter;
    private readonly ITemplateFactory _templateFactory;
    private readonly ITemplateReader _templateReader;
    private readonly IUserReader _userReader;

    public CreateSiteCommandHandler(
        AppDbContext context,
        IUserReader userReader,
        ITemplateReader templateReader,
        ISiteWriter siteWriter,
        ITemplateFactory templateFactory)
    {
        _context = context;
        _userReader = userReader;
        _templateReader = templateReader;
        _siteWriter = siteWriter;
        _templateFactory = templateFactory;
    }

    public async Task<CreateSiteResponse> HandleAsync(
        CreateSiteRequest request,
        int userId)
    {
        var user = await _userReader.GetByIdAsync(userId);

        if (user == null)
            throw new InvalidOperationException("User not found.");

        var template = await _templateReader.GetByIdAsync(
            (int)request.TemplateType);

        if (template == null)
            throw new InvalidOperationException("Template not found.");

        var jsonData = request.Data;

        var templateData = _templateFactory.Create(
            request.TemplateType,
            jsonData);

        var slug = $"site-{Guid.NewGuid():N}";

        var site = new Site
        {
            UserId = userId,
            TemplateId = (int)request.TemplateType,
            PublicSlug = slug,
            IsPublished = false,
            CreatedAt = DateTime.UtcNow
        };

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            _siteWriter.Add(site);

            await _context.SaveChangesAsync();

            templateData.SiteId = site.Id;
            await templateData.SaveAsync(_context);

            await transaction.CommitAsync();

            return new CreateSiteResponse
            {
                SiteId = site.Id,
                PublicSlug = site.PublicSlug,
                IsPublished = site.IsPublished
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}