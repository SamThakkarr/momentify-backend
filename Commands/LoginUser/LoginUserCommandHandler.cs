using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
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
    private readonly IConfiguration _configuration;

    public LoginUserCommandHandler(
        IUserReader userReader,
        IPasswordHasher<User> passwordHasher,
        IConfiguration configuration)
    {
        _userReader = userReader;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
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

        var token = GenerateToken(user);

        return new LoginUserResponse
        {
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            Token = token
        };
    }

    private string GenerateToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"]!));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
