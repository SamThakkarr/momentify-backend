using momentify_backend.DTOs;
using momentify_backend.Models;
using momentify_backend.Readers;

namespace momentify_backend.Queries.GetUser;

public interface IGetUserQueryHandler
{
    Task<UserDto?> HandleAsync(int userId);
}

public class GetUserQueryHandler : IGetUserQueryHandler
{
    private readonly IUserReader _userReader;

    public GetUserQueryHandler(IUserReader userReader)
    {
        _userReader = userReader;
    }

    public async Task<UserDto?> HandleAsync(int userId)
    {
        var user = await _userReader.GetByIdAsync(userId);

        if (user == null)
        {
            return null;
        }

        return new UserDto
        {
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            CreatedAt = user.CreatedAt
        };
    }
}
