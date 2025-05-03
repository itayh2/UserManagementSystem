using Microsoft.AspNetCore.Mvc;
using UserManagementSystem.Helpers;
using UserManagementSystem.Models;

namespace UserManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersApiController(UserJsonHelper userJsonHelper) : ControllerBase
    {
        private readonly UserJsonHelper _userJsonHelper = userJsonHelper;

        // GET api/usersapi - Returns all users
        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _userJsonHelper.GetAllUsers();
            return Ok(users);
        }

        // GET api/usersapi/{id} - Get user by id
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var user = _userJsonHelper.GetAllUsers().FirstOrDefault(u => u.UserId == id);

            if (user == null) return NotFound($"User with ID {id} not found");

            return Ok(user);
        }

        // GET api/usersapi/search?firstName=Itay&lastName=Hasid
        [HttpGet("search")]
        public IActionResult GetByName(string? firstName, string? lastName)
        {
            var users = _userJsonHelper.GetAllUsers();

            // Filter by first name if provided
            if (!string.IsNullOrEmpty(firstName))
            {
                users = users.Where(u =>
                    u.Data?.FirstName.Contains(firstName, StringComparison.CurrentCultureIgnoreCase) == true).ToList();
            }

            // Filter by last name if provided
            if (!string.IsNullOrEmpty(lastName))
            {
                users = users.Where(u =>
                    u.Data?.LastName.Contains(lastName, StringComparison.CurrentCultureIgnoreCase) == true).ToList();
            }

            if (!users.Any())
            {
                return NotFound("No matching users found");
            }

            return Ok(users);
        }

        // POST api/usersapi - Create new user
        [HttpPost]
        public IActionResult Create([FromBody] User newUser)
        {
            var users = _userJsonHelper.GetAllUsers();

            if (users.Any(u => u.UserName == newUser.UserName))
                return Conflict("Username already exists");

            // Generate new ID
            newUser.UserId = users.Any() ? users.Max(u => u.UserId) + 1 : 1;

            // Set creation date
            newUser.Data.CreationDate = DateTime.Now.ToString("yyyy-MM-dd");
                
            // Add user and save
            users.Add(newUser);
            _userJsonHelper.SaveAllUsers(users);

            // Return created response with the new user 
            return CreatedAtAction(nameof(GetById), new { id = newUser.UserId }, newUser);
        }
    }
}
