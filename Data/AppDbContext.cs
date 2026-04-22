using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Had360back.Models;

namespace Had360back.Data;

public class DbContextApp : IdentityDbContext<User, Role, int>
{
    public DbContextApp(DbContextOptions<DbContextApp> options) : base(options)
    {
    }


    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Organization> Organizations { get; set; } = null!;
    public DbSet<CommunityPost> CommunityPosts { get; set; } = null!;
    public DbSet<PostMedia> PostMedia { get; set; } = null!;
    public DbSet<Report> Reports { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Landmark> Landmarks { get; set; } = null!;
    public DbSet<LandmarkCategory> LandmarkCategories { get; set; } = null!;
    public DbSet<LandmarkMedia> LandmarkMedia { get; set; } = null!;
    public DbSet<Favorite> Favorites { get; set; } = null!;
    public DbSet<Event> Events { get; set; } = null!;
    public DbSet<EventMedia> EventMedia { get; set; } = null!;
    public DbSet<DiscoverContent> DiscoverContent { get; set; } = null!;
    public DbSet<Artisan> Artisans { get; set; } = null!;
    public DbSet<ArtisanMedia> ArtisanMedia { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Report>()
        .HasOne(r => r.Post)
        .WithMany(p => p.Reports)
        .HasForeignKey(r => r.PostId)
        .OnDelete(DeleteBehavior.Restrict);


        // // ENUM conversions (Convert C# Enum to lower case string for MySQL ENUM)
        // var mediaTypeConverter = new ValueConverter<MediaType, string>(
        //     v => v.ToString().ToLower(),
        //     v => (MediaType)Enum.Parse(typeof(MediaType), v, true));

        // modelBuilder.Entity<PostMedia>()
        //     .Property(pm => pm.MediaType)
        //     .HasConversion(mediaTypeConverter);

        // modelBuilder.Entity<LandmarkMedia>()
        //     .Property(lm => lm.MediaType)
        //     .HasConversion(mediaTypeConverter);

        // modelBuilder.Entity<EventMedia>()
        //     .Property(em => em.MediaType)
        //     .HasConversion(mediaTypeConverter);

        // modelBuilder.Entity<ArtisanMedia>()
        //     .Property(am => am.MediaType)
        //     .HasConversion(mediaTypeConverter);

        // // Default values
        // //modelBuilder.Entity<Role>().Property(r => r.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        // modelBuilder.Entity<User>().Property(u => u.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        // modelBuilder.Entity<CommunityPost>().Property(cp => cp.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        // modelBuilder.Entity<CommunityPost>().Property(cp => cp.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAddOrUpdate();
        // modelBuilder.Entity<PostMedia>().Property(pm => pm.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        // modelBuilder.Entity<Report>().Property(r => r.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        // modelBuilder.Entity<Category>().Property(c => c.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        // modelBuilder.Entity<Landmark>().Property(l => l.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        // modelBuilder.Entity<Landmark>().Property(l => l.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAddOrUpdate();
        // modelBuilder.Entity<LandmarkMedia>().Property(lm => lm.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        // modelBuilder.Entity<Favorite>().Property(f => f.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        // modelBuilder.Entity<Organization>().Property(o => o.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        // modelBuilder.Entity<Organization>().Property(o => o.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAddOrUpdate();
        // modelBuilder.Entity<Event>().Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        // modelBuilder.Entity<Event>().Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAddOrUpdate();
        // modelBuilder.Entity<EventMedia>().Property(em => em.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        // modelBuilder.Entity<DiscoverContent>().Property(dc => dc.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        // modelBuilder.Entity<DiscoverContent>().Property(dc => dc.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAddOrUpdate();
        // modelBuilder.Entity<DiscoverContent>().Property(dc => dc.IsPublished).HasDefaultValue(false);
        // modelBuilder.Entity<Artisan>().Property(a => a.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        // modelBuilder.Entity<Artisan>().Property(a => a.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAddOrUpdate();
        // modelBuilder.Entity<ArtisanMedia>().Property(am => am.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        // modelBuilder.Entity<ArtisanMedia>().Property(am => am.IsCover).HasDefaultValue(false);

        // // Unique constraints
        //// modelBuilder.Entity<Role>().HasIndex(r => r.RoleName).IsUnique();
        // modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        // modelBuilder.Entity<Category>().HasIndex(c => c.CategoryNameAr).IsUnique();
        // modelBuilder.Entity<Category>().HasIndex(c => c.CategoryNameEn).IsUnique();
        // modelBuilder.Entity<Report>().HasIndex(r => new { r.UserId, r.PostId }).IsUnique();

        // // Composite primary keys
        // modelBuilder.Entity<LandmarkCategory>().HasKey(lc => new { lc.LandmarkId, lc.CategoryId });
        // modelBuilder.Entity<Favorite>().HasKey(f => new { f.UserId, f.LandmarkId });

        // // One-to-one relationship extension
        // modelBuilder.Entity<User>()
        //     .HasOne(u => u.Organization)
        //     .WithOne(o => o.User)
        //     .HasForeignKey<Organization>(o => o.UserId)
        //     .OnDelete(DeleteBehavior.Cascade);

        // //// Relationships and Cascade Deletes
        // //modelBuilder.Entity<User>()
        // //    .HasOne(u => u.Role)
        // //    .WithMany(r => r.Users)
        // //    .HasForeignKey(u => u.RoleId)
        // //    .OnDelete(DeleteBehavior.Restrict);

        // modelBuilder.Entity<CommunityPost>()
        //     .HasOne(p => p.User)
        //     .WithMany(u => u.CommunityPosts)
        //     .HasForeignKey(p => p.UserId)
        //     .OnDelete(DeleteBehavior.Cascade);

        // modelBuilder.Entity<PostMedia>()
        //     .HasOne(pm => pm.Post)
        //     .WithMany(p => p.PostMedia)
        //     .HasForeignKey(pm => pm.PostId)
        //     .OnDelete(DeleteBehavior.Cascade);

        // modelBuilder.Entity<Report>()
        //     .HasOne(r => r.User)
        //     .WithMany(u => u.Reports)
        //     .HasForeignKey(r => r.UserId)
        //     .OnDelete(DeleteBehavior.Cascade);

        // modelBuilder.Entity<Report>()
        //     .HasOne(r => r.Post)
        //     .WithMany(p => p.Reports)
        //     .HasForeignKey(r => r.PostId)
        //     .OnDelete(DeleteBehavior.Cascade);

        // modelBuilder.Entity<LandmarkCategory>()
        //     .HasOne(lc => lc.Landmark)
        //     .WithMany(l => l.LandmarkCategories)
        //     .HasForeignKey(lc => lc.LandmarkId)
        //     .OnDelete(DeleteBehavior.Cascade);

        // modelBuilder.Entity<LandmarkCategory>()
        //     .HasOne(lc => lc.Category)
        //     .WithMany(c => c.LandmarkCategories)
        //     .HasForeignKey(lc => lc.CategoryId)
        //     .OnDelete(DeleteBehavior.Cascade);

        // modelBuilder.Entity<LandmarkMedia>()
        //     .HasOne(lm => lm.Landmark)
        //     .WithMany(l => l.LandmarkMedia)
        //     .HasForeignKey(lm => lm.LandmarkId)
        //     .OnDelete(DeleteBehavior.Cascade);

        // modelBuilder.Entity<Favorite>()
        //     .HasOne(f => f.User)
        //     .WithMany(u => u.Favorites)
        //     .HasForeignKey(f => f.UserId)
        //     .OnDelete(DeleteBehavior.Cascade);

        // modelBuilder.Entity<Favorite>()
        //     .HasOne(f => f.Landmark)
        //     .WithMany(l => l.Favorites)
        //     .HasForeignKey(f => f.LandmarkId)
        //     .OnDelete(DeleteBehavior.Cascade);

        // modelBuilder.Entity<Event>()
        //     .HasOne(e => e.Organization)
        //     .WithMany(o => o.Events)
        //     .HasForeignKey(e => e.OrgUserId)
        //     .OnDelete(DeleteBehavior.Cascade);

        // modelBuilder.Entity<EventMedia>()
        //     .HasOne(em => em.Event)
        //     .WithMany(e => e.EventMedia)
        //     .HasForeignKey(em => em.EventId)
        //     .OnDelete(DeleteBehavior.Cascade);

        // modelBuilder.Entity<ArtisanMedia>()
        //     .HasOne(am => am.Artisan)
        //     .WithMany(a => a.ArtisanMedia)
        //     .HasForeignKey(am => am.ArtisanId)
        //     .OnDelete(DeleteBehavior.Cascade);
    }
}
