using Newtonsoft.Json;
using UserManagementSystem.Models;

namespace UserManagementSystem.Helpers;
using System.Web;
public class UserJsonHelper
{
    private readonly IWebHostEnvironment _environment;

    public UserJsonHelper(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    private string GetFilePath()
    {
        return Path.Combine(_environment.ContentRootPath, "AppData", "Users.json");
    }

    public List<User> GetAllUsers()
    {
        var path = GetFilePath();
        var json = File.ReadAllText(path);
        var root = JsonConvert.DeserializeObject<UserRoot>(json);
        return root.Users;
    }

    public void SaveAllUsers(List<User> users)
    {
        var path = GetFilePath();
        var root = new UserRoot { Users = users };
        var json = JsonConvert.SerializeObject(root, Formatting.Indented);
        File.WriteAllText(path, json);
    }
}

