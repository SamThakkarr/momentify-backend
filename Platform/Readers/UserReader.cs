using Microsoft.EntityFrameworkCore;
using momentify_backend.Data;
using momentify_backend.Platform.Database.Models;

namespace momentify_backend.Platform.Readers;

public interface IUserReader
{
    Task<User?> GetByIdAsync(int userId);
    Task<User?> GetByEmailAsync(string email);
}

public class UserReader : IUserReader
{
    private readonly AppDbContext _context;

    public UserReader(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int userId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(x => x.Id == userId);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var normalizedEmail = email.Trim();

        return await _context.Users
            .FirstOrDefaultAsync(x => x.Email.ToLower() == normalizedEmail.ToLower());
    }
}