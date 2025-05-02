using Microsoft.AspNetCore.Mvc;
using UserManagementSystem.Helpers;
using UserManagementSystem.Models;

namespace UserManagementSystem.Controllers
{
    public class UsersController(UserJsonHelper userJsonHelper, ILogger<UsersController> logger) : Controller
    {
        private readonly UserJsonHelper _userJsonHelper = userJsonHelper;
        private readonly ILogger<UsersController> _logger = logger;

        // GET: Users
        public IActionResult Index(string searchTerm, string status)
        {
            var users = _userJsonHelper.GetAllUsers();

            if (!string.IsNullOrEmpty(status))
            {
                bool isActive = status == "active";
                users = users.Where(u => u.Active == isActive).ToList();
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                users = users.Where(u =>
                    (!string.IsNullOrEmpty(u.UserName) && u.UserName.ToLower().Contains(searchTerm)) ||
                    (!string.IsNullOrEmpty(u.Data?.Email) && u.Data.Email.ToLower().Contains(searchTerm)) ||
                    (!string.IsNullOrEmpty(u.Data?.Phone) && u.Data.Phone.Contains(searchTerm))
                ).ToList();
            }

            ViewBag.SearchTerm = searchTerm;
            ViewBag.Status = status;
            return View(users);
        }
        // GET: Users/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // POST:Users/Create
        public IActionResult Create(User newUser)
        {
            if (string.IsNullOrWhiteSpace(newUser.Password))
            {
                ModelState.AddModelError("Password", "Password is required");
                TempData["Error"] = "Please correct the highlighted fields.";
                return View(newUser);
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please correct the highlighted fields.";
                return View(newUser);
            }

            var users = _userJsonHelper.GetAllUsers();
            if (users.Any(u => u.UserName == newUser.UserName))
            {
                ModelState.AddModelError("UserName", "Username already exists");
                _logger.LogWarning("Attempt to create user with existing username: {UserName}", newUser.UserName);
                TempData["Error"] = "Username already exists";

                return View(newUser);
            }

            try
            {
                newUser.UserId = users.Any() ? users.Max(u => u.UserId) + 1 : 1;
                newUser.Data.CreationDate = DateTime.Now.ToString("yyyy-MM-dd");
                users.Add(newUser);
                _userJsonHelper.SaveAllUsers(users);

                _logger.LogInformation("User created: {UserName} - ", newUser.UserName);
                TempData["Success"] = $"User '{newUser.UserName}' was created successfully!";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while saving the user");
                _logger.LogError(ex, "Failed to create user: {UserName}", newUser.UserName);
                TempData["Error"] = "An unexpected error occurred.";
                return View(newUser);
            }
        }

        public IActionResult Edit(int id)
        {
            var users = _userJsonHelper.GetAllUsers();
            var user = users.FirstOrDefault(u => u.UserId == id);
            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, User editedUser)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please correct the highlighted fields.";
                return View(editedUser);
            }

            var users = _userJsonHelper.GetAllUsers();
            var user = users.FirstOrDefault(u => u.UserId == id);
            if (user == null)
            {
                _logger.LogWarning("Attempt to edit a user whose ID does not exist - {UserID}", id);
                TempData["Error"] = $"User with ID {id} was not found.";
                return NotFound();
            }

            if (users.Any(u => u.UserId != id && u.UserName == editedUser.UserName))
            {
                ModelState.AddModelError("UserName", "Username already exists");
                _logger.LogWarning("Attempt to change username to existing one: {UserName}", editedUser.UserName);
                TempData["Error"] = "Username already exists. Please choose another.";
                return View(editedUser);
            }

            try
            {
                user.UserName = editedUser.UserName;
                user.Active = editedUser.Active;
                user.Data.FirstName = editedUser.Data.FirstName;
                user.Data.LastName = editedUser.Data.LastName;
                user.Data.Phone = editedUser.Data.Phone;
                user.Data.Email = editedUser.Data.Email;

                _userJsonHelper.SaveAllUsers(users);
                _logger.LogInformation("User updated: {UserName}", user.UserName);
                TempData["Success"] = $"User '{editedUser.UserName}' was updated successfully.";

                return RedirectToAction(nameof(Index));

            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while updating the user.");
                _logger.LogError(ex, "Failed to update user: {UserName}", editedUser.UserName);
                TempData["Error"] = "An error occurred while updating the user.";

                return View(editedUser);
            }
        }

        public IActionResult Delete(int id)
        {
            var users = _userJsonHelper.GetAllUsers();
            var user = users.FirstOrDefault(u => u.UserId == id);
            if (user == null) return NotFound();

            return View(user);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var users = _userJsonHelper.GetAllUsers();
            var user = users.FirstOrDefault(u => u.UserId == id);
            if (user == null)
            {
                _logger.LogWarning("Attempting to delete a user whose ID does not exist - {UserID}", id);
                TempData["Error"] = $"User with ID {id} not found for deletion.";

                return NotFound();
            }

            try
            {
                users.Remove(user);
                _userJsonHelper.SaveAllUsers(users);

                _logger.LogInformation("User deleted: {UserName} - ", user.UserName);
                TempData["Success"] = $"User '{user.UserName}' was deleted successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the user.");
                _logger.LogError(ex, "Failed to delete user with ID: {UserID}", id);
                TempData["Error"] = "An error occurred while deleting the user.";

                return View(user);
            }
        }
    }
}
