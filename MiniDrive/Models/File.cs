namespace MiniDrive.Models;

public class File
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = default!;

    public int? FolderId { get; set; }
    public Folder? Folder { get; set; }

    public string Name { get; set; } = default!;
    public string? Extension { get; set; }
    public long? Size { get; set; }

    public string Status { get; set; } = "pending";

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }
    public User? DeletedByUser { get; set; }
}