
using MiniDrive.Data;
using MiniDrive.Models;

namespace MiniDrive.Services;

public interface IUserService
{
    DriveUser Create(string username);
    DriveUser GetById(int id);
}
public class UserService
{

    private AppDbContext _appDbContext;

    

}