using System.ComponentModel.DataAnnotations;

namespace APIDemo.Model
{
    public class User // encapsulates the payload for a request to our endpoint
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Email { get; set; }
    }
}
