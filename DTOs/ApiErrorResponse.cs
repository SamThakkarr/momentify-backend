namespace momentify_backend.DTOs;

public class ApiErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = null!;
}

