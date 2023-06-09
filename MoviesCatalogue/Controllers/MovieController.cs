﻿using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MoviesCatalogue.Classes;
using MoviesCatalogue.Classes.Wrappers;
using MoviesCatalogue.Context;
using MoviesCatalogue.Models;

namespace MoviesCatalogue.Controllers
{
    [Route("movie")]
    [ApiController]
    public class MovieController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _memoryCache;

        public MovieController(AppDbContext context, IMemoryCache memoryCache)
        {
            _context = context;
            _memoryCache = memoryCache;
        }

        //GET All Movies
        [HttpPost("get")]
        public async Task<ActionResult<List<CustomMovie>>> Get(MovieFilters filters)
        {
            List<CustomMovie> movieList = new();

            try
            {
                string cacheKey = GenerateCacheKeyFromObject(filters);

                //Validate paginated parameters
                int PageNumber = filters.PageNumber > 0 ? filters.PageNumber : 1;
                int PageSize = filters.PageSize > 0 ? filters.PageSize : 4;

                int TotalRecords = await _context.Movies.CountAsync();
                decimal PagesQuotient = TotalRecords / PageSize;
                int TotalPages = (int)Math.Ceiling(PagesQuotient);

                if (_memoryCache.TryGetValue(cacheKey, out List<CustomMovie> cacheMovies))
                {
                    return Ok(new PaginatedResponse<List<CustomMovie>>(cacheMovies, PageNumber, PageSize, TotalPages, TotalRecords));
                }

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(1))
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));

                if (TotalRecords == 0)
                {
                    _memoryCache.Set(cacheKey, movieList, cacheEntryOptions);
                    return Ok(new PaginatedResponse<List<CustomMovie>>(movieList, PageNumber, PageSize, TotalPages, TotalRecords));
                }

                int SkipRows = (PageNumber - 1) * PageSize;

                var moviesWithRatings = from movie in _context.Movies
                                        join user in _context.Users on movie.UserId equals user.Id
                                        join rating in _context.RatedMovies on movie.Id equals rating.MovieId into movieRatings
                                        select new
                                        {
                                            movie,
                                            ratings = movieRatings.ToList(),
                                            user
                                        };

                if (!string.IsNullOrEmpty(filters.SearchText))
                {
                    moviesWithRatings = moviesWithRatings.Where(m => m.movie.Name.Contains(filters.SearchText) || m.movie.Synopsis.Contains(filters.SearchText));
                }

                if (!string.IsNullOrEmpty(filters.Category))
                {
                    moviesWithRatings = moviesWithRatings.Where(m => m.movie.Category.Equals(filters.Category));
                }

                if (filters.YearOfRelease is not null && filters.YearOfRelease > 0)
                {
                    moviesWithRatings = moviesWithRatings.Where(m => m.movie.ReleaseYear.Equals(filters.YearOfRelease));
                }

                var filteredMoviesWithRatings = moviesWithRatings
                                                 .Select(m => new CustomMovie
                                                 {
                                                     Id = m.movie.Id,
                                                     Name = m.movie.Name,
                                                     Synopsis = m.movie.Synopsis,
                                                     ImagePoster = m.movie.ImagePoster,
                                                     ReleaseYear = m.movie.ReleaseYear,
                                                     Category = m.movie.Category,
                                                     CreatedDate = m.movie.CreatedDate,
                                                     Rate = m.ratings.Any() ? Math.Round(m.ratings.Average(r => r.Rate), 2) : 0,
                                                     CreatedByUser = new CreatedByUser { Id = m.user.Id, Name = m.user.Name }
                                                 })
                                                 .OrderBy(m => m.ReleaseYear)
                                                 .ThenBy(m => m.Name)
                                                 .ThenBy(m => m.CreatedDate)
                                                 .ThenBy(m => m.Rate)
                                                 .Skip(SkipRows)
                                                 .Take(PageSize)
                                                 .ToList();

                movieList = filteredMoviesWithRatings;

                _memoryCache.Set(cacheKey, movieList, cacheEntryOptions);

                return Ok(new PaginatedResponse<List<CustomMovie>>(movieList, PageNumber, PageSize, TotalPages, TotalRecords));

            }
            catch (Exception error)
            {
                string message = "An error occurred while getting the movies.";
                return BadRequest(new Response<List<CustomMovie>>(message, error.Message, movieList));
            }
        }


        // PUT
        [Authorize(Policy = "AdminPermission")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMovie(int id, CreateMovie movie)
        {
            string message = "Could not update movie.";

            if (id == 0 || movie.Id == 0)
            {
                return BadRequest(new Response<dynamic>(message, "Empty id field.", ""));
            }

            if (id != movie.Id)
            {
                return BadRequest(new Response<dynamic>(message, "The id field does not match.", ""));
            }

            if (!string.IsNullOrEmpty(movie.Name))
            {
                return BadRequest(new Response<dynamic>(message, "Movie name is required.", ""));
            }

            if (!string.IsNullOrEmpty(movie.Category))
            {
                return BadRequest(new Response<dynamic>(message, "Movie category is required.", ""));
            }

            if (movie.ReleaseYear == 0)
            {
                return BadRequest(new Response<dynamic>(message, "Year of release is required.", ""));
            }

            var entityObject = await _context.Movies.FindAsync(id);

            if (entityObject is null)
            {
                return NotFound(new Response<dynamic>(message, "Movie not found.", ""));
            }

            entityObject.Name = movie.Name;
            entityObject.ReleaseYear = movie.ReleaseYear;
            entityObject.Synopsis = movie.Synopsis;
            entityObject.ImagePoster = movie.ImagePoster;
            entityObject.Category = movie.Category;

            _context.Entry(entityObject).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MovieExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST
        [Authorize(Policy = "AdminPermission")]
        [HttpPost]
        public async Task<ActionResult<dynamic>> PostMovie(CreateMovie movie)
        {
            string message = "Could not created movie.";

            try
            {
                if (!string.IsNullOrEmpty(movie.Name))
                {
                    return BadRequest(new Response<dynamic>(message, "Movie name is required.", ""));
                }

                if (!string.IsNullOrEmpty(movie.Category))
                {
                    return BadRequest(new Response<dynamic>(message, "Movie category is required.", ""));
                }

                if (movie.ReleaseYear == 0)
                {
                    return BadRequest(new Response<dynamic>(message, "Year of release is required.", ""));
                }

                ClaimsIdentity? identity = HttpContext.User.Identity as ClaimsIdentity;

                if (identity == null)
                {
                    return NotFound(new Response<dynamic>(message, "User not found", ""));
                }

                int userId = Jwt.GetClaimId(identity);

                Movie entityObject = new()
                {
                    Name = movie.Name,
                    ReleaseYear = movie.ReleaseYear,
                    Synopsis = movie.Synopsis,
                    ImagePoster = movie.ImagePoster,
                    CreatedDate = DateTime.UtcNow,
                    UserId = userId,
                    Category = movie.Category
                };

                _context.Movies.Add(entityObject);
                await _context.SaveChangesAsync();

                return Ok(new Response<Movie>("Successfully created movie.", entityObject));
            }
            catch (Exception error)
            {
                return BadRequest(new Response<dynamic>(message, error.Message, ""));
            }

        }

        // DELETE
        [Authorize(Policy = "AdminPermission")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMovie(int id)
        {
            
            string message = "Could not deleted movie.";

            if (id == 0)
            {
                return NotFound(new Response<dynamic>(message, "Empty id.", ""));
            }

            var movie = await _context.Movies.FindAsync(id);
            
            if (movie == null)
            {
                return NotFound(new Response<string>(message, "Movie not found.", ""));
            }

            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MovieExists(int id)
        {
            return (_context.Movies?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        private static string GenerateCacheKeyFromObject<TEntity>(TEntity movieObj)
        {
            var key = string.Empty;

            if (movieObj != null) {

                PropertyInfo[] properties = movieObj.GetType().GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    if (property.GetValue(movieObj) != null)
                    {
                        key += property.GetValue(movieObj) + "-";
                    }
                }
            }

            return key;
        }
    }
}
