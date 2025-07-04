using R_Factory_BE.Models;

namespace R_Factory_BE.Auth
{
    public class AuthResponse(Users user, string token)
    {
        public int UserId { get; set; } = user.Id;
        public string Username { get; set; } = user.Username;
        public string Fullname { get; set; } = user.Fullname;
        public string? AccessToken { get; set; } = token;
        public string Redirect { get; set; } = user.RoleId == 0 ? "/managers" : "/users";
        //public string? RefreshToken { get; set; } = refreshToken;
    }
}