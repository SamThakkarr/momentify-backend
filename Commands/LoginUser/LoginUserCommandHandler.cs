using Microsoft.AspNetCore.Identity;
using momentify_backend.DTOs;
using momentify_backend.Models;
using momentify_backend.Readers;

namespace momentify_backend.Commands.LoginUser;

public interface ILoginUserCommandHandler
{
    Task<LoginUserResponse?> HandleAsync(LoginUserRequest request);
}

public class LoginUserCommandHandler : ILoginUserCommandHandler
{
    private readonly IUserReader _userReader;
    private readonly IPasswordHasher<User> _passwordHasher;

    public LoginUserCommandHandler(
        IUserReader userReader,
        IPasswordHasher<User> passwordHasher)
    {
        _userReader = userReader;
        _passwordHasher = passwordHasher;
    }

    public async Task<LoginUserResponse?> HandleAsync(
        LoginUserRequest request)
    {
        var user = await _userReader.GetByEmailAsync(request.Email);

        if (user == null)
            return null;

        var passwordResult = _passwordHasher.VerifyHashedPassword(
            user,
            user.SecurityKey,
            request.SecurityKey);

        if (passwordResult == PasswordVerificationResult.Failed)
            return null;

        return new LoginUserResponse
        {
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email
        };
    }
}

