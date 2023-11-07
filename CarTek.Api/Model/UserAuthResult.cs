namespace CarTek.Api.Model
{
    public class UserAuthResult
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public User Identity { get; set; }
        public bool IsDriver { get; set; }
    }
}
