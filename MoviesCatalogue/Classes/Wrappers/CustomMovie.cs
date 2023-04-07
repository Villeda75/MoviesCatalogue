using MoviesCatalogue.Models;

namespace MoviesCatalogue.Classes.Wrappers
{
    public class CustomMovie
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ReleaseYear { get; set; }
        public string Synopsis { get; set; }
        public string? ImagePoster { get; set; }
        public string Category { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? Rate { get; set; }
        public CreatedByUser CreatedByUser { get; set; }
    }
}
