using Microsoft.EntityFrameworkCore;
using MiniDrive.Data;
using MiniDrive.Models;

namespace MiniDrive.Services;

public record CreateFolderRequest(int UserId, string Name, int? ParentId);
public record GetFolderRequest(int UserId, int? FolderId);

public record DeleteFolderRequest(int UserId, int FolderId);

public record ChildFolderResponse(int Id, string Name, DateTime CreatedAt);
public record FolderResponse(int Id, string Name, int? ParentId, DateTime CreatedAt, IEnumerable<ChildFolderResponse> SubFolders);

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

        if (user is null)
            /* Claro que no sistema real, não falamos que o user não existe :P */
            throw new KeyNotFoundException("User cannot be found!");

        DriveFolder? parent = null;
        if (request.ParentId.HasValue)
        {
            parent = await _db.Folders
                .FirstOrDefaultAsync(f => f.Id == request.ParentId.Value, cancellationToken);

            if (parent is null)
                throw new KeyNotFoundException("Parent folder not found!");
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

        return new FolderResponse(folder.Id, folder.Name, folder.ParentId, folder.CreatedAt, []);
    }

    public async Task<FolderResponse> GetAsync(
        GetFolderRequest request,
        CancellationToken cancellationToken = default)
    {

        if (request.UserId <= 0)
            throw new ApplicationException("User Id cannot be empty!");

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
            /* Claro que no sistema real, não falamos que o user não existe :P */
            throw new KeyNotFoundException("User cannot be found!");

        DriveFolder? folder;
        if (request.FolderId is null)
        {
            /* User em raiz -> Retornamos uma pasta virtual com os filhos do lvl0 */
            var rootChildren = await _db.Folders
                .Where(f => f.UserId == request.UserId
                        && f.ParentId == null
                        && f.DeletedAt == null)
                .Select(f => new ChildFolderResponse(f.Id, f.Name, f.CreatedAt))
                .ToListAsync(cancellationToken);

            return new FolderResponse(
                Id: 0,
                Name: "Root",
                ParentId: null,
                CreatedAt: DateTime.UtcNow,
                SubFolders: rootChildren
            );
        }

        /* Pasta real -> Valida dono */
        folder = await _db.Folders
            .Where(f => f.Id == request.FolderId
                    && f.UserId == request.UserId
                    && f.DeletedAt == null)
            .Include(f => f.Children.Where(c => c.DeletedAt == null)) /* <-- Filhos diretos */
            .FirstOrDefaultAsync(cancellationToken);

        if (folder is null)
            throw new KeyNotFoundException("Folder not found!");

        var subFolders = folder.Children
            .Select(c => new ChildFolderResponse(c.Id, c.Name, c.CreatedAt));

        return new FolderResponse(folder.Id, folder.Name, folder.ParentId, folder.CreatedAt, subFolders);
    }

    public async Task<bool> DeleteAsync(
        DeleteFolderRequest request,
        CancellationToken cancellationToken = default)
    {
        
        var folder = await _db.Folders
            .FirstOrDefaultAsync(
                f => f.Id == request.FolderId
                    && f.UserId == request.UserId
                    && f.DeletedAt == null,
                cancellationToken);

        if (folder is null)
            throw new KeyNotFoundException("Folder not found!");

        var allFolderIds = new List<int> { folder.Id };
        var queue = new Queue<int>();
        queue.Enqueue(folder.Id);

        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();

            var childIds = await _db.Folders
                .Where(f => f.ParentId == currentId && f.DeletedAt == null)
                .Select(f => f.Id)
                .ToListAsync(cancellationToken);

            foreach (var id in childIds)
            {
                allFolderIds.Add(id);
                queue.Enqueue(id);
            }
        }

        var now = DateTime.UtcNow;

        /* Soft delete em batch das pastas */
        await _db.Folders
            .Where(f => allFolderIds.Contains(f.Id))
            .ExecuteUpdateAsync(s => s
                .SetProperty(f => f.DeletedAt, now)
                .SetProperty(f => f.DeletedBy, request.UserId),
            cancellationToken);

        /* Soft delete em batch dos arquivos */
        await _db.Files
            .Where(f => f.FolderId != null
                    && allFolderIds.Contains(f.FolderId!.Value)
                    && f.DeletedAt == null)
            .ExecuteUpdateAsync(s => s
                .SetProperty(f => f.DeletedAt, now)
                .SetProperty(f => f.DeletedBy, request.UserId),
            cancellationToken);

        return true;
    }
}
