using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using momentify_backend.Data;
using momentify_backend.DTOs;
using momentify_backend.Interfaces;
using momentify_backend.Models;
using momentify_backend.Readers;
using momentify_backend.Writers;

namespace momentify_backend.Commands.CreateSite;

public interface ICreateSiteCommandHandler
{
    Task<CreateSiteResponse> HandleAsync(CreateSiteRequest request);
}

public class CreateSiteCommandHandler : ICreateSiteCommandHandler
{
    private readonly AppDbContext _context;
    private readonly IUserReader _userReader;
    private readonly ITemplateReader _templateReader;
    private readonly ISiteWriter _siteWriter;
    private readonly ITemplateFactory _templateFactory;

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
        CreateSiteRequest request)
    {
        var user = await _userReader.GetByIdAsync(request.UserId);

        if (user == null)
            throw new InvalidOperationException("User not found.");

        var template = await _templateReader.GetByIdAsync(
            (int)request.TemplateType);

        if (template == null)
            throw new InvalidOperationException("Template not found.");

        var slug = $"site-{Guid.NewGuid():N}";

        var site = new Site
        {
            UserId = request.UserId,
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

            var jsonData = JsonSerializer.SerializeToElement(
                request.Data);

            var templateData = _templateFactory.Create(
                request.TemplateType,
                jsonData);

            switch (templateData)
            {
                case OurStoryData ourStoryData:
                    ourStoryData.SiteId = site.Id;
                    await ourStoryData.SaveAsync(_context);
                    break;

                case MemoriesData memoriesData:
                    memoriesData.SiteId = site.Id;
                    await memoriesData.SaveAsync(_context);
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Template data handling is not implemented for {request.TemplateType}.");
            }

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
