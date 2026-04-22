using Microsoft.EntityFrameworkCore;
using MiniDrive.Data;
using MiniDrive.Models;

namespace MiniDrive.Services;

public record CreateUserRequest(string Username);
public record GetUserRequest(string? Username, int? Id);
public record UserResponse(int Id, string Username, DateTime CreatedAt);

public interface IUserService
{
    Task<UserResponse> CreateAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default);

    Task<UserResponse> GetAsync(
        GetUserRequest request,
        CancellationToken cancellationToken = default);
}

public class UserService : IUserService
{

    private readonly AppDbContext _db;

    public UserService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<UserResponse> CreateAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            throw new ArgumentException("Username is mandatory!", nameof(request));

        var username = request.Username.Trim();

        if (username.Length > 40)
            throw new ArgumentException("Username excede 40 caracteres!", nameof(request));

        var exists = await _db.Users
            .AnyAsync(u => u.Username == username, cancellationToken);

        if (exists)
            throw new InvalidOperationException("Username is already in use!");

        var user = new DriveUser
        {
            Username = username,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        return new UserResponse(user.Id, user.Username, user.CreatedAt);
    }

    public async Task<UserResponse> GetAsync(
        GetUserRequest request,
        CancellationToken cancellationToken = default)
    {
        int? id = request.Id is int parsedId && parsedId > 0 ? parsedId : null;
        var hasId = id.HasValue;
        var hasUsername = !string.IsNullOrWhiteSpace(request.Username);

        if (!hasId && !hasUsername)
            throw new ArgumentException("Enter the ID or Username to search for the user!", nameof(request));

        var query = _db.Users.AsNoTracking();

        DriveUser? user;
        if (hasId && hasUsername)
        {
            var username = request.Username!.Trim();
            user = await query.FirstOrDefaultAsync(
                u => u.Id == id!.Value && u.Username == username,
                cancellationToken);
        }
        else if (hasId)
        {
            user = await query.FirstOrDefaultAsync(u => u.Id == id!.Value, cancellationToken);
        }
        else
        {
            var username = request.Username!.Trim();
            user = await query.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
        }

        if (user is null)
            throw new KeyNotFoundException("User not found!");

        return new UserResponse(user.Id, user.Username, user.CreatedAt);
    }

}