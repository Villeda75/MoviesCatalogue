using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoviesCatalogue.Models
{
    public class RatedMovie
    {
        [Key]
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Rate { get; set; }

        // One-to-many relation with Users
        public int? UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        // One-to-many relation with Movies
        public int? MovieId { get; set; }
        [ForeignKey("MovieId")]
        public virtual Movie? Movie { get; set; }
    }
}
