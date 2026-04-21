using MiniDrive.Data;
using MiniDrive.Models;

namespace MiniDrive.Services;

public record CreateUserRequest(string Username);
public record UserResponse(int Id, string Username, DateTime CreatedAt);

public interface IUserService
{
    Task<UserResponse> CreateAsync(
        CreateUserRequest request,
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

}