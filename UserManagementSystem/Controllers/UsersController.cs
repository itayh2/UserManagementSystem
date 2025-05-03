using Microsoft.AspNetCore.Mvc;
using UserManagementSystem.Helpers;
using UserManagementSystem.Models;

namespace UserManagementSystem.Controllers
{
    public class UsersController(UserJsonHelper userJsonHelper, ILogger<UsersController> logger) : Controller
    {
        private readonly UserJsonHelper _userJsonHelper = userJsonHelper;
        private readonly ILogger<UsersController> _logger = logger;

        // GET: Users - Main user listing page
        public IActionResult Index(string searchTerm, string status)
        {
            var users = _userJsonHelper.GetAllUsers();

            // Filter by active/inactive status
            if (!string.IsNullOrEmpty(status))
            {
                bool isActive = status == "active";
                users = users.Where(u => u.Active == isActive).ToList();
            }

            // Search by username, email or phone
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

        // GET: Users/Create - Shows the create user form
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // POST:Users/Create - Handles the create user form submission
        public IActionResult Create(User newUser)
        {
            // Check if password exists
            if (string.IsNullOrWhiteSpace(newUser.Password))
            {
                ModelState.AddModelError("Password", "Password is required");
                TempData["Error"] = "Please fix the errors and try again";
                return View(newUser);
            }
            // Check model validation
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please correct the highlighted fields";
                return View(newUser);
            }

            var users = _userJsonHelper.GetAllUsers();

            // Check for duplicate username
            if (users.Any(u => u.UserName == newUser.UserName))
            {
                ModelState.AddModelError("UserName", "Username already exists");
                _logger.LogWarning("Attempt to create user with existing username: {UserName}", newUser.UserName);
                TempData["Error"] = "This username is taken, please choose another one";

                return View(newUser);
            }

            try
            {
                // Generate new user ID
                newUser.UserId = users.Any() ? users.Max(u => u.UserId) + 1 : 1;
                newUser.Data.CreationDate = DateTime.Now.ToString("yyyy-MM-dd");
                users.Add(newUser);
                _userJsonHelper.SaveAllUsers(users);

                _logger.LogInformation($"New user created: {newUser.UserName}");
                TempData["Success"] = $"User '{newUser.UserName}' was created successfully!";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Sorry, couldn't save the user");
                _logger.LogError(ex, "Failed to create user: {UserName}", newUser.UserName);
                TempData["Error"] = "Something went wrong, please try again";
                return View(newUser);
            }
        }
        // GET: Users/Edit - Shows the edit user form
        public IActionResult Edit(int id)
        {
            var users = _userJsonHelper.GetAllUsers();
            var user = users.FirstOrDefault(u => u.UserId == id);
            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // POST: Users/Edit/{id} - Handles user edit form submission
        public IActionResult Edit(int id, User editedUser)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Some fields have invalid values, please check and try again";
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

            // Check if new username conflicts with existing one
            if (users.Any(u => u.UserId != id && u.UserName == editedUser.UserName))
            {
                ModelState.AddModelError("UserName", "Username already exists");
                _logger.LogWarning("Attempt to change username to existing one: {UserName}", editedUser.UserName);
                TempData["Error"] = "Username already exists. Please choose another.";
                return View(editedUser);
            }

            try
            {
                // Update user fields
                user.UserName = editedUser.UserName;
                user.Active = editedUser.Active;
                user.UserGroupId = editedUser.UserGroupId;
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
                ModelState.AddModelError(string.Empty, "Couldn't save changes to the user");
                _logger.LogError(ex, "Failed to update user: {UserName}", editedUser.UserName);
                TempData["Error"] = "Failed to save changes";

                return View(editedUser);
            }
        }

        // GET: Users/Delete - Shows the delete confirmation page
        public IActionResult Delete(int id)
        {
            var users = _userJsonHelper.GetAllUsers();
            var user = users.FirstOrDefault(u => u.UserId == id);
            if (user == null) return NotFound();

            return View(user);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        //POST: Users/Delete/{id} - Handles user deletion
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
                // Remove the user
                users.Remove(user);
                _userJsonHelper.SaveAllUsers(users);

                _logger.LogInformation("User deleted: {UserName} - ", user.UserName);
                TempData["Success"] = $"User '{user.UserName}' was deleted successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Delete operation failed");
                _logger.LogError(ex, "Failed to delete user with ID: {UserID}", id);
                TempData["Error"] = "Something went wrong while deleting this user";

                return View(user);
            }
        }
    }
}
