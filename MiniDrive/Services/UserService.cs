using Microsoft.EntityFrameworkCore;
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

public class UserService
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
        
    }

}