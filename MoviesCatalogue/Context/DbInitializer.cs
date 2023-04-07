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
                    Category = "Fantasy",
                    ReleaseYear = 2001,
                    CreatedDate = DateTime.UtcNow,
                    UserId = 1
                });

                b.HasData(new Movie
                {
                    Id = 2,
                    Name = "Harry Potter and the Chamber of Secrets",
                    Synopsis = "Ever since Harry Potter had come home for the summer, the Dursleys had been so mean and hideous that all Harry wanted was to get back to the Hogwarts School for Witchcraft and Wizardry. ",
                    Category = "Fantasy",
                    ReleaseYear = 2002,
                    CreatedDate = DateTime.UtcNow,
                    UserId = 1
                });

                b.HasData(new Movie
                {
                    Id = 3,
                    Name = "Rocky",
                    Synopsis = "Rocky is a small-time Philadelphia boxer going nowhere, until an unbelievable shot to fight the world heavyweight champion lights a fire inside him.",
                    Category = "Sports drama",
                    ReleaseYear = 1976,
                    CreatedDate = DateTime.UtcNow,
                    UserId = 2
                });

                b.HasData(new Movie
                {
                    Id = 4,
                    Name = "Bee Movie",
                    Synopsis = "Barry, a worker bee stuck in a dead-end job making honey, sues humans when he learns they've been stealing bees' nectar all along.",
                    Category = "Kids",
                    ReleaseYear = 2007,
                    CreatedDate = DateTime.UtcNow,
                    UserId = 2
                });

                b.HasData(new Movie
                {
                    Id = 5,
                    Name = "Fast Five",
                    Synopsis = "Brian and Mia break Dom out of prison and head to Brazil to put together a racing team -- and take on a drug dealer who wants to kill them.",
                    Category = "Action & Adventure",
                    ReleaseYear = 2011,
                    CreatedDate = DateTime.UtcNow,
                    UserId = 1
                });

            });

            _builder.Entity<RatedMovie>(a =>
            {
                a.HasData(new RatedMovie
                {
                    Id = 1,
                    MovieId = 1,
                    UserId = 1,
                    Rate = 9,
                });

                a.HasData(new RatedMovie
                {
                    Id = 2,
                    MovieId = 2,
                    UserId = 2,
                    Rate = 7,
                });
            });
        }
    }
}
