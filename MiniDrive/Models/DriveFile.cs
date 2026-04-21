namespace MiniDrive.Models;

public class DriveFile
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public DriveUser User { get; set; } = default!;

    public int? FolderId { get; set; }
    public DriveFolder? Folder { get; set; }

    public string Name { get; set; } = default!;
    public string? Extension { get; set; }
    public long? Size { get; set; }

    public string Status { get; set; } = "pending";

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }
    public DriveUser? DeletedByUser { get; set; }
}