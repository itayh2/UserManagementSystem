namespace UserManagementSystem.Models
{
    public class User
    {
        public int UserId { get; set; }
        public bool Active { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int? UserGroupId { get; set; }
        public UserData Data { get; set; } = new UserData();
    }
}
