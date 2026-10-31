using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using momentify_backend.Data;
using momentify_backend.Platform.Database.Models;
using momentify_backend.Platform.Readers;
using momentify_backend.Platform.Writers;
using momentify_backend.Services.User.Contracts;

namespace momentify_backend.Commands.CreateUser;

public interface ICreateUserCommandHandler
{
    Task<CreateUserResponse> HandleAsync(CreateUserRequest request);
}

public class CreateUserCommandHandler : ICreateUserCommandHandler
{
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IUserReader _userReader;
    private readonly IUserWriter _userWriter;

    public CreateUserCommandHandler(
        AppDbContext context,
        IUserReader userReader,
        IUserWriter userWriter,
        IPasswordHasher<User> passwordHasher,
        IConfiguration configuration)
    {
        _context = context;
        _userReader = userReader;
        _userWriter = userWriter;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    public async Task<CreateUserResponse> HandleAsync(
        CreateUserRequest request)
    {
        var existingUser =
            await _userReader.GetByEmailAsync(request.Email);

        if (existingUser != null)
            throw new InvalidOperationException(
                "A user with this email already exists.");

        var user = new User
        {
            Name = request.Name,
            Email = request.Email.Trim().ToLowerInvariant(),
            PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber)
                ? null
                : request.PhoneNumber.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        user.SecurityKey = _passwordHasher.HashPassword(
            user,
            request.SecurityKey);

        _userWriter.Add(user);

        await _context.SaveChangesAsync();

        var token = GenerateToken(user);

        return new CreateUserResponse
        {
            UserId = user.Id,
            CreatedAt = user.CreatedAt,
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
            _configuration["Jwt:Issuer"],
            _configuration["Jwt:Audience"],
            claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}