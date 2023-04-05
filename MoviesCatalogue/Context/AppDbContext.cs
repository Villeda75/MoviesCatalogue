using Microsoft.EntityFrameworkCore;
using MoviesCatalogue.Models;

namespace MoviesCatalogue.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<RatedMovie> RatedMovies { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Seed database with authors and books for demo
            new DbInitializer(builder).Seed();
        }
    }
}
