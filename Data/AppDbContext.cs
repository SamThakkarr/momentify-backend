using Microsoft.EntityFrameworkCore;
using momentify_backend.Platform.Database.Models;

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
    public DbSet<MemoriesData> MemoriesData => Set<MemoriesData>();
    public DbSet<Memory> Memories => Set<Memory>();
    public DbSet<MemoryPhoto> MemoryPhotos => Set<MemoryPhoto>();
    public DbSet<Photo> Photos => Set<Photo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(x => x.Email)
            .IsUnique();

        // User → Sites
        modelBuilder.Entity<Site>()
            .HasOne<User>()
            .WithMany(x => x.Sites)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // User → Photos
        modelBuilder.Entity<Photo>()
            .HasOne<User>()
            .WithMany()
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

        // Site → MemoriesData
        modelBuilder.Entity<MemoriesData>()
            .HasKey(x => x.SiteId);

        modelBuilder.Entity<MemoriesData>()
            .HasOne<Site>()
            .WithOne()
            .HasForeignKey<MemoriesData>(x => x.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Memory>()
            .Property(x => x.Date)
            .HasColumnType("date");

        // MemoriesData → Memories
        modelBuilder.Entity<Memory>()
            .HasOne<MemoriesData>()
            .WithMany(x => x.Memories)
            .HasForeignKey(x => x.MemoriesDataSiteId)
            .OnDelete(DeleteBehavior.Cascade);

        // Memory → MemoryPhotos
        modelBuilder.Entity<MemoryPhoto>()
            .HasOne<Memory>()
            .WithMany(x => x.Photos)
            .HasForeignKey(x => x.MemoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // MemoryPhoto → Photo
        modelBuilder.Entity<MemoryPhoto>()
            .HasOne(x => x.Photo)
            .WithMany()
            .HasForeignKey(x => x.PhotoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Photo DisplayOrder validation
        modelBuilder.Entity<Photo>()
            .ToTable(table => table.HasCheckConstraint(
                "CK_Photos_DisplayOrder",
                "\"DisplayOrder\" = -1 OR \"DisplayOrder\" >= 1"));
    }
}