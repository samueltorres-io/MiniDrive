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
        string folderName = string.IsNullOrWhiteSpace(request.Name)
            ? "New Folder"
            : request.Name.Trim();
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