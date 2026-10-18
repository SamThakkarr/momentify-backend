namespace momentify_backend.Models;

public class User
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string SecurityKey { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<Site> Sites { get; set; } = new List<Site>();
}

