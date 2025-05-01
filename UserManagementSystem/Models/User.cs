namespace UserManagementSystem.Models
{
    public class User
    {
        public int UserID { get; set; }
        public bool Active { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int? UserGroupID { get; set; }
        public UserData Data { get; set; }
    }
}
