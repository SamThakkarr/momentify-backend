namespace momentify_backend.DTOs;

public class LoginUserRequest
{
    public string Email { get; set; } = null!;
    public string SecurityKey { get; set; } = null!;
}

