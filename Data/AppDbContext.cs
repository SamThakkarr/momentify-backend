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

    public DbSet<OurStoryData> OurStoryData => Set<OurStoryData>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User → Sites
        modelBuilder.Entity<Site>()
            .HasOne<User>()
            .WithMany(x => x.Sites)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Template → Sites
        modelBuilder.Entity<Site>()
            .HasOne<Template>()
            .WithMany()
            .HasForeignKey(x => x.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        // Site → OurStoryData
        modelBuilder.Entity<OurStoryData>()
            .HasKey(x => x.SiteId);

        modelBuilder.Entity<OurStoryData>()
            .HasOne<Site>()
            .WithOne()
            .HasForeignKey<OurStoryData>(x => x.SiteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}