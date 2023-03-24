using System.ComponentModel.DataAnnotations;

namespace APIDemo.Model
{
    public class AuthenticationRequest
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}