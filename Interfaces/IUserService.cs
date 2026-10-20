using momentify_backend.Models;

namespace momentify_backend.Interfaces;

public interface IUserService
{
    Task<User> SaveUserAsync(User user);

    Task<User?> UpdateUserAsync(User user);

    Task<bool> DeleteUserAsync(int userId);
}

