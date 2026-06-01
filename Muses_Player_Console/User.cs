using System;

namespace Muses_Player_Console
{
    public class User
    {
        public string UserID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public User()
        {
            // Default constructor
        }
        public User(string userID, string username, string email, string password)
        {
            UserID = userID;
            Username = username;
            Email = email;
            Password = password;
        }
    }
}