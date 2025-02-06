namespace WebApi.Models
{
    public enum Role
    {
        User,
        Admin
    }

    public class User
    {
        public int Id { get; set; }
        public string Pseudo { get; set; }
        public string Password { get; set; }
        public Role Role { get; set; }
        public User() { }
         public User(int id, string pseudo, string password, Role role)
        {
            Id = id;
            Pseudo = pseudo;
            Password = password;
            Role = role;
        }
    }
    public class UserInfo
    {
        public string Pseudo { get; set; }
        public string Password { get; set; }
    }/////dans models
    public class UserCreation
    {
        public string Pseudo { get; set; }
        public string Password { get; set; }
        public Role Role { get; set; }
    }

    public class UserWithToken
    {
        public User User { get; set; }    // Contient les informations utilisateur
        public string Token { get; set; } // Contient le token JWT
    }
}

