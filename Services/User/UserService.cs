using momentify_backend.Commands.CreateUser;
using momentify_backend.Commands.LoginUser;
using momentify_backend.Platform.Queries.GetUser;
using momentify_backend.Services.User.Contracts;

namespace momentify_backend.Services.User;

public interface IUserService
{
    Task<CreateUserResponse> SaveUserAsync(CreateUserRequest request);
    Task<UserDto?> GetUserAsync(int userId);
    Task<LoginUserResponse?> LoginUserAsync(LoginUserRequest request);
}

public class UserService : IUserService
{
    private readonly ICreateUserCommandHandler _createUserHandler;
    private readonly IGetUserQueryHandler _getUserHandler;
    private readonly ILoginUserCommandHandler _loginUserHandler;

    public UserService(
        ICreateUserCommandHandler createUserHandler,
        IGetUserQueryHandler getUserHandler,
        ILoginUserCommandHandler loginUserHandler)
    {
        _createUserHandler = createUserHandler;
        _getUserHandler = getUserHandler;
        _loginUserHandler = loginUserHandler;
    }

    public async Task<CreateUserResponse> SaveUserAsync(
        CreateUserRequest request) =>
        await _createUserHandler.HandleAsync(request);

    public async Task<UserDto?> GetUserAsync(int userId) =>
        await _getUserHandler.HandleAsync(userId);

    public async Task<LoginUserResponse?> LoginUserAsync(
        LoginUserRequest request) =>
        await _loginUserHandler.HandleAsync(request);
}
