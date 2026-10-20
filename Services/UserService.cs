using Microsoft.EntityFrameworkCore;
using momentify_backend.Data;
using momentify_backend.Interfaces;
using momentify_backend.Models;

namespace momentify_backend.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User> SaveUserAsync(User user)
    {
        user.CreatedAt = DateTime.UtcNow;

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<User?> UpdateUserAsync(User user)
    {
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(x => x.Id == user.Id);

        if (existingUser == null)
            return null;

        existingUser.Name = user.Name;
        existingUser.Email = user.Email;
        existingUser.PhoneNumber = user.PhoneNumber;
        existingUser.SecurityKey = user.SecurityKey;

        await _context.SaveChangesAsync();

        return existingUser;
    }

    public async Task<bool> DeleteUserAsync(int userId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Id == userId);

        if (user == null)
            return false;

        _context.Users.Remove(user);

        await _context.SaveChangesAsync();

        return true;
    }
}

