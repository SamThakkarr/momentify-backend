using Microsoft.EntityFrameworkCore;
using momentify_backend.Models;

namespace momentify_backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Template> Templates => Set<Template>();

    public DbSet<Site> Sites => Set<Site>();
}

