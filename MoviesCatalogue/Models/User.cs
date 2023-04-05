using System.ComponentModel.DataAnnotations;

namespace MoviesCatalogue.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }

        // One-to-many relationship with Movies
        public List<RatedMovie>? RatedMovies { get; set; }
    }
}
