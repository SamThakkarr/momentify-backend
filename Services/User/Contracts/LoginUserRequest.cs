namespace momentify_backend.Services.User.Contracts;

public class LoginUserRequest
{
    public string Email { get; set; } = null!;
    public string SecurityKey { get; set; } = null!;
}

