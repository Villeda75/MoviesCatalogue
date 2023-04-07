using Microsoft.Build.Framework;

namespace MoviesCatalogue.Classes.Wrappers
{
    public class CreateMovie
    {
        [Required]
        public string Name { get; set; }
        public int ReleaseYear { get; set; }
        [Required]
        public string Synopsis { get; set; }
        public string? ImagePoster { get; set; }
        [Required]
        public string Category { get; set; }
    }
}
