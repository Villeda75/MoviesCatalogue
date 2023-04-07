using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
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

        // GET
        [HttpGet]
        public async Task<ActionResult<dynamic>> GetMovies(MovieFilters filters)
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

                var movies = from m in _context.Movies
                             join usr in _context.Users
                             on m.UserId equals usr.Id
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
                                 CreatedDate = m.CreatedDate,
                                 CreatedByUser = new CreatedByUser { 
                                     Id = usr.Id, 
                                     Name = usr.Name 
                                 }
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

                _memoryCache.Set(cacheKey, movieList, cacheEntryOptions);
                return Ok(new PaginatedResponse<List<CustomMovie>>(movieList, PageNumber, PageSize, TotalPages, TotalRecords));

            }
            catch (Exception error)
            {
                string message = "An error occurred while getting the movies.";
                return BadRequest(new Response<dynamic>(message, error.Message, movieList));
            }
        }


        // PUT
        [Authorize(Policy = "AdminPermission")]
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

        // POST
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

        // DELETE
        [Authorize(Policy = "AdminPermission")]
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
