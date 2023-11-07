using Microsoft.AspNetCore.Identity;

namespace CarTek.Api.Model
{
    #nullable disable

    public class User
    {
        public long Id { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }

        public string Login { get; set; }

        public bool IsAdmin { get; set; }

        public bool IsDispatcher { get; set; }

        public string Phone { get; set; }

        public string Password { get; set; }

        public string? RefreshToken { get; set; }

        public DateTime RefreshTokenExpiryTime { get; set; }

        public ICollection<Questionary> Questionaries { get; set; }
    }
}
