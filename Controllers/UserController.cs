using Microsoft.AspNetCore.Mvc;
using momentify_backend.DTOs;
using momentify_backend.Services;

namespace momentify_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserRequest request)
    {
        try
        {
            var response = await _userService.SaveUserAsync(request);

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUser(int userId)
    {
        var user = await _userService.GetUserAsync(userId);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginUserRequest request)
    {
        var result = await _userService.LoginUserAsync(request);

        if (result == null)
            return Unauthorized("Invalid email or password.");

        return Ok(result);
    }
}
