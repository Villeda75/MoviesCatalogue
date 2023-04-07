using System.ComponentModel.DataAnnotations;

namespace MoviesCatalogue.Classes.Wrappers
{
    public class MovieFilters
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchText { get; set; }
        public string? Category { get; set; }
        public int? YearOfRelease { get; set; }
    }
}
