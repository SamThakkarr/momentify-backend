namespace momentify_backend.DTOs;

public class LoginUserResponse
{
    public int UserId { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
}

