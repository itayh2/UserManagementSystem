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
            if (!File.Exists(path))
            {
                _logger.LogWarning("Users file not found at path: {Path}", path);
                // If file doesn't exist, return empty list
                return new List<User>();
            }
            var json = File.ReadAllText(path);

            // Try to deserialize the JSON
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

            // Create root object and serialize
            var root = new UserRoot { Users = users };
            var json = JsonConvert.SerializeObject(root, Formatting.Indented);

            // Write to file
            File.WriteAllText(path, json);
            _logger.LogInformation($"Saved {users.Count} users to file");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save users to file");
            throw;
        }
    }
}

