using momentify_backend.Platform.Queries.GetTemplates;

namespace momentify_backend.Services.Template;

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