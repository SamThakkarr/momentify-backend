namespace momentify_backend.DTOs;

public class CreateUserResponse
{
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Token { get; set; } = null!;
}
