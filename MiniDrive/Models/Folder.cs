namespace MiniDrive.Models;

public class Folder
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = default!;

    public string Name { get; set; } = default!;

    public int? ParentId { get; set; }
    public Folder? Parent { get; set; }
    public ICollection<Folder> Children { get; set; } = new List<Folder>();

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }
    public User? DeletedByUser { get; set; }

    public ICollection<File> Files { get; set; } = new List<File>();
}