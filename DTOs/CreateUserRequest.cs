namespace momentify_backend.DTOs;

public class CreateUserRequest
{
    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string SecurityKey { get; set; } = null!;
}

