using momentify_backend.Data;
using momentify_backend.Models;

namespace momentify_backend.Writers;

public interface ITemplateWriter
{
    void Add(Template template);
}

public class TemplateWriter : ITemplateWriter
{
    private readonly AppDbContext _context;

    public TemplateWriter(AppDbContext context)
    {
        _context = context;
    }

    public void Add(Template template)
    {
        _context.Templates.Add(template);
    }
}

