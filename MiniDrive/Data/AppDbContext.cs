using Microsoft.EntityFrameworkCore;
using MiniDrive.Models;

namespace MiniDrive.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<DriveUser> Users { get; set; } = null!;
    public DbSet<DriveFile> Files { get; set; } = null!;
    public DbSet<DriveFolder> Folders { get; set; } = null!;
}
