using Microsoft.AspNetCore.Identity;
using momentify_backend.Data;
using momentify_backend.DTOs;
using momentify_backend.Models;
using momentify_backend.Readers;
using momentify_backend.Writers;

namespace momentify_backend.Commands.CreateUser;

public interface ICreateUserCommandHandler
{
    Task<CreateUserResponse> HandleAsync(CreateUserRequest request);
}

public class CreateUserCommandHandler : ICreateUserCommandHandler
{
    private readonly AppDbContext _context;
    private readonly IUserReader _userReader;
    private readonly IUserWriter _userWriter;
    private readonly IPasswordHasher<User> _passwordHasher;

    public CreateUserCommandHandler(
        AppDbContext context,
        IUserReader userReader,
        IUserWriter userWriter,
        IPasswordHasher<User> passwordHasher)
    {
        _context = context;
        _userReader = userReader;
        _userWriter = userWriter;
        _passwordHasher = passwordHasher;
    }

    public async Task<CreateUserResponse> HandleAsync(
        CreateUserRequest request)
    {
        var existingUser =
            await _userReader.GetByEmailAsync(request.Email);

        if (existingUser != null)
        {
            throw new InvalidOperationException(
                "A user with this email already exists.");
        }

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            CreatedAt = DateTime.UtcNow
        };

        user.SecurityKey = _passwordHasher.HashPassword(
            user,
            request.SecurityKey);

        _userWriter.Add(user);

        await _context.SaveChangesAsync();

        return new CreateUserResponse
        {
            UserId = user.Id,
            CreatedAt = user.CreatedAt
        };
    }
}
