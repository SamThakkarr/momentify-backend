namespace momentify_backend.Services.ErrorHandling;

public class ApiErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = null!;
}

