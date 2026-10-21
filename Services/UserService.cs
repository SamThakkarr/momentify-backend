using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using momentify_backend.Commands.CreateUser;
using momentify_backend.DTOs;
using momentify_backend.Queries.GetUser;

namespace momentify_backend.Services;

public interface IUserService
{
    Task<CreateUserResponse> SaveUserAsync(CreateUserRequest request);
    Task<UserDto?> GetUserAsync(int userId);
}

public class UserService : IUserService
{
    private readonly ICreateUserCommandHandler _createUserHandler;
    private readonly IGetUserQueryHandler _getUserHandler;

    public UserService(
        ICreateUserCommandHandler createUserHandler,
        IGetUserQueryHandler getUserHandler)
    {
        _createUserHandler = createUserHandler;
        _getUserHandler = getUserHandler;
    }

    public async Task<CreateUserResponse> SaveUserAsync(
        CreateUserRequest request)
    {
        return await _createUserHandler.HandleAsync(request);
    }

    public async Task<UserDto?> GetUserAsync(int userId)
    {
        return await _getUserHandler.HandleAsync(userId);
    }
}
