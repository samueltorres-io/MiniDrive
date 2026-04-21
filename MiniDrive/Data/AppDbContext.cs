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

        /* User */
        modelBuilder.Entity<DriveUser>(e =>
        {
            e.ToTable("users");
            e.HasKey(u => u.Id);
            e.Property(u => u.Username)
                .HasMaxLength(40)
                .IsRequired();
            e.HasIndex(u => u.Username).IsUnique();
        });

    }
}
