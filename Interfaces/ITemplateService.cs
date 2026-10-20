using momentify_backend.DTOs;
using momentify_backend.Models;

namespace momentify_backend.Interfaces;

public interface ITemplateService
{
    Task<IEnumerable<TemplateDto>> GetAvailableTemplatesAsync();
}
