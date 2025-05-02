using Microsoft.AspNetCore.Mvc;
using UserManagementSystem.Helpers;
using UserManagementSystem.Models;

namespace UserManagementSystem.Controllers
{
    public class UsersController(UserJsonHelper userJsonHelper) : Controller
    {
        private readonly UserJsonHelper _userJsonHelper = userJsonHelper;

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
            var users = _userJsonHelper.GetAllUsers();
            if (users.Any(u => u.UserName == newUser.UserName))
            {
                ModelState.AddModelError("UserName", "Username already exists");
                return View(newUser);
            }

            newUser.UserId = users.Any() ? users.Max(u => u.UserId) + 1 : 1;
            newUser.Data.CreationDate = DateTime.Now.ToString("yyyy-MM-dd");
            users.Add(newUser);
            _userJsonHelper.SaveAllUsers(users);

            return RedirectToAction(nameof(Index));
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
            var users = _userJsonHelper.GetAllUsers();
            var user = users.FirstOrDefault(u => u.UserId == id);
            if (user == null) return NotFound();

            if (users.Any(u => u.UserId != id && u.UserName == editedUser.UserName))
            {
                ModelState.AddModelError("UserName", "Username already exists");
                return View(editedUser);
            }

            user.UserName = editedUser.UserName;
            user.Active = editedUser.Active;
            user.Data.FirstName = editedUser.Data.FirstName;
            user.Data.LastName = editedUser.Data.LastName;
            user.Data.Phone = editedUser.Data.Phone;
            user.Data.Email = editedUser.Data.Email;

            _userJsonHelper.SaveAllUsers(users);

            return RedirectToAction(nameof(Index));
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
            if (user == null) return NotFound();

            users.Remove(user);
            _userJsonHelper.SaveAllUsers(users);

            return RedirectToAction(nameof(Index));
        }
    }
}
