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

        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _userJsonHelper.GetAllUsers();
            return Ok(users);
        }
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var user = _userJsonHelper.GetAllUsers().FirstOrDefault(u => u.UserId == id);
            if (user == null) return NotFound($"User with ID {id} not found");

            return Ok(user);
        }

        [HttpGet("search")]
        public IActionResult GetByName(string? firstName, string? lastName)
        {
            var users = _userJsonHelper.GetAllUsers();

            if (!string.IsNullOrEmpty(firstName))
            {
                users = users.Where(u =>
                    u.Data?.FirstName.Contains(firstName, StringComparison.CurrentCultureIgnoreCase) == true).ToList();
            }

            if (!string.IsNullOrEmpty(lastName))
            {
                users = users.Where(u =>
                    u.Data?.LastName.Contains(lastName, StringComparison.CurrentCultureIgnoreCase) == true).ToList();
            }

            if (!users.Any())
            {
                return NotFound("No users found matching");
            }

            return Ok(users);
        }

        [HttpPost]
        public IActionResult Create([FromBody] User newUser)
        {
            var users = _userJsonHelper.GetAllUsers();

            if (users.Any(u => u.UserName == newUser.UserName))
                return Conflict("Username already exists");

            newUser.UserId = users.Any() ? users.Max(u => u.UserId) + 1 : 1;

            newUser.Data.CreationDate = DateTime.Now.ToString("yyyy-MM-dd");

            users.Add(newUser);
            _userJsonHelper.SaveAllUsers(users);

            return CreatedAtAction(nameof(GetById), new { id = newUser.UserId }, newUser);
        }
    }
}
