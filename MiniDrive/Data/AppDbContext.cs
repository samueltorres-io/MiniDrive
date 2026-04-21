using Microsoft.EntityFrameworkCore;
using MiniDrive.Models;

namespace MiniDrive.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<DriveUser> Users { get; set; } = null!;
    public DbSet<DriveFile> Files { get; set; } = null!;
    public DbSet<DriveFolder> Folders { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        /* Users */
        modelBuilder.Entity<DriveUser>(e =>
        {
            e.ToTable("users");
            e.HasKey(u => u.Id);
            e.Property(u => u.Username)
                .HasMaxLength(40)
                .IsRequired();
            e.HasIndex(u => u.Username).IsUnique();
        });

        /* Folders */
        modelBuilder.Entity<DriveFolder>(e =>
        {
            e.ToTable("folders");
            e.HasKey(f => f.Id);
            e.Property(f => f.Name).HasMaxLength(256).IsRequired();
            e.HasIndex(f => f.UserId);
            e.HasIndex(f => f.ParentId);
            e.HasOne(f => f.User)
                .WithMany(u => u.Folders)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(f => f.DeletedByUser)
                .WithMany()
                .HasForeignKey(f => f.DeletedBy)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(f => f.Parent)
                .WithMany(f => f.Children)
                .HasForeignKey(f => f.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

    }
}
