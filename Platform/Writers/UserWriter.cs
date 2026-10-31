using momentify_backend.Data;
using momentify_backend.Platform.Database.Models;

namespace momentify_backend.Platform.Writers;

public interface IUserWriter
{
    void Add(User user);
}

public class UserWriter : IUserWriter
{
    private readonly AppDbContext _context;

    public UserWriter(AppDbContext context)
    {
        _context = context;
    }

    public void Add(User user)
    {
        _context.Users.Add(user);
    }
}

