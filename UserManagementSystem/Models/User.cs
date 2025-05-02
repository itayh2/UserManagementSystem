using System.ComponentModel.DataAnnotations;

namespace UserManagementSystem.Models
{
    public class User
    {
        public int UserId { get; set; }
        public bool Active { get; set; }
        [Required(ErrorMessage = "UserName is required")]
        [StringLength(50)]
        public string UserName { get; set; } = string.Empty;
        [MinLength(4)]
        public string? Password { get; set; }
        public int? UserGroupId { get; set; }
        [Required]
        public UserData Data { get; set; } = new UserData();
    }
}
