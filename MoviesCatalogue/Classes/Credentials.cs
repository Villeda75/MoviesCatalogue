using Microsoft.Build.Framework;

namespace MoviesCatalogue.Classes
{
    public class Credentials
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
