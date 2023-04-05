using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoviesCatalogue.Models
{
    public class Movie
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int ReleaseYear { get; set; }
        public string Synopsis { get; set; }
        public string? ImagePoster { get; set; }
        public string Category { get; set; }   
        public DateTime CreatedDate { get; set; }

        // One-to-many relation with user
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
