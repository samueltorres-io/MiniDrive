namespace MiniDrive.Models;

public class DriveUser
{
    public int Id { get; set; }
    public string Username { get; set; } = default!;
    public DateTime CreatedAt { get; set; }

    public ICollection<DriveFolder> Folders { get; set; } = new List<DriveFolder>();
    public ICollection<DriveFile> Files { get; set; } = new List<DriveFile>();
}