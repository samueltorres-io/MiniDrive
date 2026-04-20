namespace MiniDrive.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = default!;
    public DateTime CreatedAt { get; set; }

    public ICollection<Folder> Folders { get; set; } = new List<Folder>();
    public ICollection<File> Files { get; set; } = new List<File>();
}