using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MoviesCatalogue.Classes.Wrappers;
using MoviesCatalogue.Context;
using MoviesCatalogue.Models;
using Newtonsoft.Json;
//using CustomMovie = MoviesCatalogue.Classes.Wrappers.Movie;

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

        // GET: api/Movies
        [HttpGet]
        public async Task<ActionResult<dynamic>> GetMovies(MovieFilters filters)
        {
            List<CustomMovie> movieList = new();

            try
            {
                int TotalRecords = await _context.Movies.CountAsync();

                if (TotalRecords == 0)
                {
                    return Ok(new PaginatedResponse<List<CustomMovie>>(movieList, 1, 0, 0, 0));
                }

                //Validate paginated parameters
                int PageNumber = filters.PageNumber > 0 ? filters.PageNumber : 1; 
                int PageSize = filters.PageSize > 0 ? filters.PageSize : 4; 

                var movies = from m in _context.Movies
                             join mr in _context.RatedMovies
                             on m.Id equals mr.MovieId into moviesRating
                             from ratedMovies in moviesRating.DefaultIfEmpty()
                             orderby m.ReleaseYear, m.Name, m.CreatedDate, ratedMovies.Rate
                             select new CustomMovie
                             { 
                                 Id = m.Id,
                                 Name = m.Name,
                                 Synopsis = m.Synopsis,
                                 ImagePoster = m.ImagePoster,
                                 ReleaseYear = m.ReleaseYear,
                                 Category = m.Category,
                                 Rate = ratedMovies.Rate,
                                 CreatedDate = m.CreatedDate
                             };

                if (!string.IsNullOrEmpty(filters.SearchText))
                {
                    movies = movies.Where(m => m.Name.Contains(filters.SearchText) || m.Synopsis.Contains(filters.SearchText));
                }

                if (!string.IsNullOrEmpty(filters.Category))
                {
                    movies = movies.Where(m => m.Category.Equals(filters.Category));
                }

                if (filters.YearOfRelease is not null && filters.YearOfRelease > 0)
                {
                    movies = movies.Where(m => m.ReleaseYear.Equals(filters.YearOfRelease));
                }

                int SkipRows = (PageNumber - 1) * PageSize;

                movieList = await movies.Skip(SkipRows).Take(PageSize).ToListAsync();

                decimal PagesQuotient = TotalRecords / PageSize;
                int TotalPages = (int)Math.Ceiling(PagesQuotient);

                return Ok(new PaginatedResponse<List<CustomMovie>>(movieList, PageNumber, PageSize, TotalPages, TotalRecords));

            }
            catch (Exception error)
            {
                string message = "An error occurred while getting the movies.";
                return BadRequest(new Response<dynamic>(message, error.Message, movieList));
            }
        }

       
        // PUT: api/Movies/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMovie(int id, Movie movie)
        {
            if (id != movie.Id)
            {
                return BadRequest();
            }

            _context.Entry(movie).State = EntityState.Modified;

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

        // POST: api/Movies
        [Authorize(Policy = "AdminPermission")]
        [HttpPost]
        public async Task<ActionResult<Movie>> PostMovie(Movie movie)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            bool isAdmin = IsAdmin(identity);

            if (isAdmin) {
                _context.Movies.Add(movie);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetMovie", new { id = movie.Id }, movie);
            }

            return Unauthorized(new
            {
                success = false,
                message = "You don't have administrator permission",
                data = ""
            });
        }

        // DELETE: api/Movies/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMovie(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }

            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MovieExists(int id)
        {
            return (_context.Movies?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        private bool IsAdmin(ClaimsIdentity identity)
        {
            bool isAdmin = false;

            try
            {

                if (identity.Claims.Any())
                {

                    var Id = identity.Claims.FirstOrDefault(x => x.Type == "Id").Value;

                    if (int.TryParse(Id, out int userId))
                    {
                        User user = _context.Users.Where(x => x.Id.Equals(userId)).FirstOrDefault();
                        
                        isAdmin = user.Role.Equals("Admin");
                    }
                }

                return isAdmin;
            }
            catch (Exception error)
            {
                return isAdmin;
            }
        }
    }
}
