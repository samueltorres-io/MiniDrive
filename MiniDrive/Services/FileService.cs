using Microsoft.EntityFrameworkCore;
using Minio;
using Minio.DataModel.Args;
using MiniDrive.Data;
using MiniDrive.Models;

namespace MiniDrive.Services;

public record UploadFileRequest(int UserId, int? FolderId, IFormFile File);
public record ListFilesRequest(int UserId, int? FolderId);
public record DownloadFileRequest(int UserId, int FileId);
public record DeleteFileRequest(int UserId, int FileId);

public record FileResponse(
    int Id,
    string Name,
    string? Extension,
    long? Size,
    string Status,
    int? FolderId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record FileDownloadResult(
    Stream Content,
    string ContentType,
    string FileName);

public interface IFileService
{
    Task<FileResponse> UploadAsync(
        UploadFileRequest request,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<FileResponse>> ListAsync(
        ListFilesRequest request,
        CancellationToken cancellationToken = default);

    Task<FileDownloadResult> DownloadAsync(
        DownloadFileRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        DeleteFileRequest request,
        CancellationToken cancellationToken = default);
}

public class FileService : IFileService
{
    private readonly AppDbContext _db;
    private readonly IMinioClient _minio;
    private readonly string _bucket;

    private const long MaxFileSize = 512L * 1024 * 1024; /* Default 512 MB */

    public FileService(AppDbContext db, IMinioClient minio, IConfiguration config)
    {
        _db     = db;
        _minio  = minio;
        _bucket = config["Minio__Bucket"] ?? config["Minio:Bucket"] ?? config["MINIO_BUCKET"] ?? "minidrive";
    }

    public async Task<FileResponse> UploadAsync(
        UploadFileRequest request,
        CancellationToken cancellationToken = default)
    {
        /* Fail First */
        if (request.UserId <= 0)
            throw new ApplicationException("User Id cannot be empty!");

        if (request.File is null || request.File.Length == 0)
            throw new ApplicationException("File cannot be empty!");

        if (request.File.Length > MaxFileSize)
            throw new ApplicationException("File exceeds the maximum allowed size of 512 MB!");

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("User not found!");

        DriveFolder? folder = null;
        if (request.FolderId.HasValue)
        {
            folder = await _db.Folders
                .FirstOrDefaultAsync(
                    f => f.Id == request.FolderId.Value
                      && f.UserId == request.UserId
                      && f.DeletedAt == null,
                    cancellationToken)
                ?? throw new KeyNotFoundException("Folder not found!");
        }

        var originalName = Path.GetFileNameWithoutExtension(request.File.FileName).Trim();
        var extension = Path.GetExtension(request.File.FileName).TrimStart('.').ToLowerInvariant();
        var contentType = request.File.ContentType;

        /* Persiste com status "pending" antes do upload */
        var driveFile = new DriveFile
        {
            UserId = user.Id,
            User = user,
            FolderId = folder?.Id,
            Folder = folder,
            Name = originalName,
            Extension = string.IsNullOrEmpty(extension) ? null : extension,
            Size = request.File.Length,
            Status = "pending",
            CreatedAt = DateTime.UtcNow,
        };

        _db.Files.Add(driveFile);
        await _db.SaveChangesAsync(cancellationToken);

        var objectKey = BuildObjectKey(driveFile);

        try
        {
            await using var stream = request.File.OpenReadStream();

            var putArgs = new PutObjectArgs()
                .WithBucket(_bucket)
                .WithObject(objectKey)
                .WithStreamData(stream)
                .WithObjectSize(request.File.Length)
                .WithContentType(contentType);

            await _minio.PutObjectAsync(putArgs, cancellationToken);

            driveFile.Status = "active";
            driveFile.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            _db.Files.Remove(driveFile);
            await _db.SaveChangesAsync(cancellationToken);
            throw;
        }

        return ToResponse(driveFile);
    }

    public async Task<IEnumerable<FileResponse>> ListAsync(
        ListFilesRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.UserId <= 0)
            throw new ApplicationException("User Id cannot be empty!");

        var userExists = await _db.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);
        if (!userExists)
            throw new KeyNotFoundException("User not found!");

        /* Sem folderId -> raiz */
        var query = _db.Files
            .Where(f => f.UserId == request.UserId
                    && f.DeletedAt == null
                    && f.Status == "active");

        query = request.FolderId.HasValue
            ? query.Where(f => f.FolderId == request.FolderId.Value)
            : query.Where(f => f.FolderId == null);

        return await query
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => ToResponse(f))
            .ToListAsync(cancellationToken);
    }

    public async Task<FileDownloadResult> DownloadAsync(
        DownloadFileRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.UserId <= 0)
            throw new ApplicationException("User Id cannot be empty!");

        var driveFile = await _db.Files
            .FirstOrDefaultAsync(
                f => f.Id == request.FileId
                    && f.UserId == request.UserId
                    && f.DeletedAt == null
                    && f.Status == "active",
                cancellationToken)
            ?? throw new KeyNotFoundException("File not found!");

        var objectKey = BuildObjectKey(driveFile);
        var memoryStream = new MemoryStream();

        var getArgs = new GetObjectArgs()
            .WithBucket(_bucket)
            .WithObject(objectKey)
            .WithCallbackStream(async (stream, ct) =>
            {
                await stream.CopyToAsync(memoryStream, ct);
            });

        await _minio.GetObjectAsync(getArgs, cancellationToken);
        memoryStream.Seek(0, SeekOrigin.Begin);

        var fileName = string.IsNullOrEmpty(driveFile.Extension)
            ? driveFile.Name
            : $"{driveFile.Name}.{driveFile.Extension}";

        var contentType = driveFile.Extension switch
        {
            "pdf"           => "application/pdf",
            "png"           => "image/png",
            "jpg" or "jpeg" => "image/jpeg",
            "gif"           => "image/gif",
            "webp"          => "image/webp",
            "svg"           => "image/svg+xml",
            "mp4"           => "video/mp4",
            "mp3"           => "audio/mpeg",
            "zip"           => "application/zip",
            "tar" or "gz"   => "application/gzip",
            "txt"           => "text/plain",
            "csv"           => "text/csv",
            "json"          => "application/json",
            "xml"           => "application/xml",
            "docx"          => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "xlsx"          => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "pptx"          => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            _               => "application/octet-stream",
        };

        return new FileDownloadResult(memoryStream, contentType, fileName);
    }

    public async Task<bool> DeleteAsync(
        DeleteFileRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.UserId <= 0)
            throw new ApplicationException("User Id cannot be empty!");

        var driveFile = await _db.Files
            .FirstOrDefaultAsync(
                f => f.Id == request.FileId
                    && f.UserId == request.UserId
                    && f.DeletedAt == null,
                cancellationToken)
            ?? throw new KeyNotFoundException("File not found!");

        driveFile.DeletedAt = DateTime.UtcNow;
        driveFile.DeletedBy = request.UserId;

        await _db.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <summary>
    /// Monta a chave do objeto no MinIO.
    /// files/{userId}/{fileId}/{nome.ext}
    /// </summary>
    private static string BuildObjectKey(DriveFile file)
    {
        var fileName = string.IsNullOrEmpty(file.Extension)
            ? file.Name
            : $"{file.Name}.{file.Extension}";

        return $"files/{file.UserId}/{file.Id}/{fileName}";
    }

    private static FileResponse ToResponse(DriveFile f) =>
        new(f.Id, f.Name, f.Extension, f.Size, f.Status, f.FolderId, f.CreatedAt, f.UpdatedAt);
}