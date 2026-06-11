
namespace Muses_Player_Console
{
    public class User
    {
        public string? UserId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }

        public User()
        {
            // Default constructor
        }
        public User(string userId, string username, string email, string password)
        {
            UserId = userId;
            Username = username;
            Email = email;
            Password = password;
        }
    }
}