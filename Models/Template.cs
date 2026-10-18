namespace momentify_backend.Models;

public class Template
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string IconId { get; set; } = null!;

    public ICollection<Site> Sites { get; set; } = new List<Site>();
}

