using System.ComponentModel.DataAnnotations;

namespace RemoteBackupsApp.MVC.Models.Auth
{
    public class LoginRegisterViewModel
    {
        [Required, StringLength(100)]
        public string Username { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, DataType(DataType.Password), StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        [Required, DataType(DataType.Password), Compare("Password", ErrorMessage = "The passwords do not match")]
        public string ConfirmPassword { get; set; }
    }
}
