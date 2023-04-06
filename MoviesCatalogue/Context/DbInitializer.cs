using Microsoft.EntityFrameworkCore;
using MoviesCatalogue.Models;

namespace MoviesCatalogue.Context
{
    public class DbInitializer
    {
        private readonly ModelBuilder _builder;

        public DbInitializer(ModelBuilder builder)
        {
            _builder = builder;
        }

        public void Seed()
        {
            _builder.Entity<User>(a =>
            {
                a.HasData(new User
                {
                    Id = 1,
                    Name = "Josue Chacon",
                    Email = "josuechacon@gmail.com",
                    Password = "@dmi321!",
                    Role = "Admin",
                });

                a.HasData(new User
                {
                    Id = 2,
                    Name = "Alexander Villeda",
                    Email = "alexander@gmail.com",
                    Password = "us3r321!",
                    Role = "User",
                });
            });

            _builder.Entity<Movie>(b =>
            {
                b.HasData(new Movie
                {
                    Id = 1,
                    Name = "Harry Potter and the Sorcerer's Stone",
                    Synopsis = "Harry Potter's life is miserable. His parents are dead and he's stuck with his heartless relatives, who force him to live in a tiny closet under the stairs.",
                    Category = "Horror",
                    ReleaseYear = 1998,
                    CreatedDate = new DateTime(1965, 07, 31),
                    UserId = 1
                });

                b.HasData(new Movie
                {
                    Id = 2,
                    Name = "Harry Potter and the Chamber of Secrets",
                    Synopsis = "Ever since Harry Potter had come home for the summer, the Dursleys had been so mean and hideous that all Harry wanted was to get back to the Hogwarts School for Witchcraft and Wizardry. ",
                    Category = "Science fiction",
                    ReleaseYear = 2020,
                    CreatedDate = new DateTime(2023, 03, 31),
                    UserId = 2
                });
                
            });

            _builder.Entity<RatedMovie>(a =>
            {
                a.HasData(new RatedMovie
                {
                    Id = 1,
                    MovieId = 1,
                    UserId = 1,
                    Rate = 7,
                });

                a.HasData(new RatedMovie
                {
                    Id = 2,
                    MovieId = 2,
                    UserId = 2,
                    Rate = 9,
                });
            });
        }
    }
}
