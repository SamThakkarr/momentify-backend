using momentify_backend.DTOs;
using momentify_backend.Models;

namespace momentify_backend.Interfaces;

public interface IUserService
{
    Task<CreateUserResponse> SaveUserAsync(CreateUserRequest request);

    Task<User?> UpdateUserAsync(User user);

    Task<bool> DeleteUserAsync(int userId);
}
