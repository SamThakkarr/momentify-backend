using Microsoft.AspNetCore.Mvc;
using momentify_backend.Services;
using momentify_backend.Services.Template;

namespace momentify_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TemplateController : ControllerBase
{
    private readonly ITemplateService _templateService;

    public TemplateController(ITemplateService templateService)
    {
        _templateService = templateService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAvailableTemplates()
    {
        var templates = await _templateService.GetAvailableTemplatesAsync();

        return Ok(templates);
    }
}
