namespace MiniDrive.Models;

public class DriveFolder
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public DriveUser User { get; set; } = default!;

    public string Name { get; set; } = default!;

    public int? ParentId { get; set; }
    public DriveFolder? Parent { get; set; }
    public ICollection<DriveFolder> Children { get; set; } = new List<DriveFolder>();

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }
    public DriveUser? DeletedByUser { get; set; }

    public ICollection<DriveFile> Files { get; set; } = new List<DriveFile>();
}