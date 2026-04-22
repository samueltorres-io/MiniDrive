using Microsoft.EntityFrameworkCore;
using MiniDrive.Data;
using MiniDrive.Models;

namespace MiniDrive.Services;

public record CreateFolderRequest(int UserId, string Name, int? ParentId);
public record GetFolderRequest(int UserId, int? FolderId);
public record DeleteFolderRequest(int FolderId);
public record FolderResponse(int Id, string Name, int? ParentId, DateTime CreatedAt);

public interface IFolderService
{
    Task<FolderResponse> CreateAsync(
        CreateFolderRequest request,
        CancellationToken cancellationToken = default);

    Task<FolderResponse> GetAsync(
        GetFolderRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        DeleteFolderRequest request,
        CancellationToken cancellationToken = default);
}

public class FolderService : IFolderService
{

    private readonly AppDbContext _db;

    public FolderService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<FolderResponse> CreateAsync(
        CreateFolderRequest request,
        CancellationToken cancellationToken = default)
    {
        /* Failt First */
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length > 40)
            throw new ApplicationException("Folder name cannot be empty or excede 40 characters!");

        string folderName = request.Name.Trim();

        if (request.UserId <= 0)
            throw new ApplicationException("User Id cannot be empty!");

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (!user)
            /* Claro que no sistema real, não falamos que o user não existe :P */
            throw new ApplicationException("User cannot be found!");

        DriveFolder? parent = null;
        if (request.ParentId.HasValue)
        {
            parent = await _db.Folders
                .FirstOrDefaultAsync(f => f.Id == request.ParentId.Value, cancellationToken);

            if (parent is null)
                throw new ApplicationException("Parent folder not found!");
        }

        var folder = new DriveFolder
        {
            UserId = user.Id,
            User = user,
            Name = folderName,
            ParentId = parent?.Id,
            Parent = parent,
        };

        _db.Folders.Add(folder);
        await _db.SaveChangesAsync(cancellationToken);

        return new FolderResponse(folder.Id, folder.Name, folder.ParentId, folder.CreatedAt);
    }

    public async Task<FolderResponse> GetAsync(
        CreateFolderRequest request,
        CancellationToken cancellationToken = default)
    {
        
    }

    public async Task<bool> DeleteAsync(
        CreateFolderRequest request,
        CancellationToken cancellationToken = default)
    {
        return true;
    }

}