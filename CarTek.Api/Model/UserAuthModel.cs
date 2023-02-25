using System.ComponentModel.DataAnnotations;

namespace CarTek.Api.Model
{
    public class UserAuthModel
    {
        [Required(ErrorMessage = "User Name is required")]
        public string? Login { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }
    }
}
