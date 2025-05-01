using Newtonsoft.Json;
using UserManagementSystem.Models;

namespace UserManagementSystem.Helpers;
public class UserJsonHelper(IWebHostEnvironment environment, ILogger<UserJsonHelper> logger)
{
    private readonly IWebHostEnvironment _environment = environment;
    private readonly ILogger<UserJsonHelper> _logger = logger;

    private string GetFilePath()
    {
        return Path.Combine(_environment.ContentRootPath, "AppData", "Users.json");
    }

    public List<User> GetAllUsers()
    {
        try
        {
            var path = GetFilePath();
            if (Equals(!File.Exists(path)))
            {
                _logger.LogWarning("Users.json file not found at path: {Path}", path);
                return new List<User>();
            }
            var json = File.ReadAllText(path);
            var root = JsonConvert.DeserializeObject<UserRoot>(json);
            return root?.Users ?? new List<User>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading users file");
            return new List<User>();
        }
    }

    public void SaveAllUsers(List<User> users)
    {
        try
        {
            var path = GetFilePath();
            var root = new UserRoot { Users = users };
            var json = JsonConvert.SerializeObject(root, Formatting.Indented);
            File.WriteAllText(path, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving users file");
            throw;
        }
    }
}

