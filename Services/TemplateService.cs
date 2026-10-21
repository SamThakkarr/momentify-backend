using momentify_backend.DTOs;
using momentify_backend.Queries.GetTemplates;

namespace momentify_backend.Services;

public interface ITemplateService
{
    Task<IEnumerable<TemplateDto>> GetAvailableTemplatesAsync();
}

public class TemplateService : ITemplateService
{
    private readonly IGetTemplatesQueryHandler _getTemplatesHandler;

    public TemplateService(
        IGetTemplatesQueryHandler getTemplatesHandler)
    {
        _getTemplatesHandler = getTemplatesHandler;
    }

    public async Task<IEnumerable<TemplateDto>> GetAvailableTemplatesAsync()
    {
        return await _getTemplatesHandler.HandleAsync();
    }
}