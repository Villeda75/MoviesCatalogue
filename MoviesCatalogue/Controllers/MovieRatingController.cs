using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesCatalogue.Context;
using MoviesCatalogue.Models;
using MoviesCatalogue.Classes;
using MoviesCatalogue.Classes.Wrappers;

namespace MoviesCatalogue.Controllers
{
    [Route("movierating")]
    [ApiController]
    public class MovieRatingController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MovieRatingController(AppDbContext context)
        {
            _context = context;
        }

        // GET
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<RatedMovie>>> GetRatedMovies()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (identity == null)
            {
                return NotFound();
            }

            int userId = Jwt.GetClaimId(identity);

            if (userId > 0)
            {
                var moviesRated = await (
                        from rm in _context.RatedMovies
                        join m in _context.Movies on rm.MovieId equals m.Id
                        join usr in _context.Users on rm.UserId equals usr.Id
                        where rm.UserId == userId
                        select new RatedMovie
                        {
                            Id = rm.Id,
                            CreatedDate = rm.CreatedDate,
                            Rate = rm.Rate,
                            UserId = userId,
                            User = null,
                            MovieId = rm.MovieId,
                            Movie = new Movie
                            {
                                Id = m.Id,
                                Name = m.Name,
                                Synopsis = m.Synopsis,
                                Category = m.Category,
                                ReleaseYear = m.ReleaseYear,
                                ImagePoster = m.ImagePoster,
                                CreatedDate = m.CreatedDate
                            }
                            
                        }
                    ).ToListAsync();

                return Ok(moviesRated); 
            }

            return NotFound();
            
        }

        //POST
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<dynamic>> PostRatedMovie(MovieRating movieRating)
        {
            string message = "Could not rated movie.";

            try
            {
                if(movieRating.MovieId == 0 || movieRating.Rating == 0)
                {
                    return BadRequest(new Response<dynamic>(message, "Invalid movie rating object.", ""));
                }

                var identity = HttpContext.User.Identity as ClaimsIdentity;

                if (identity == null)
                {
                    return NotFound(new Response<dynamic>(message, "User not found", ""));
                }

                int userId = Jwt.GetClaimId(identity);

                if (UserHasRatedMovie(movieRating.MovieId).Result)
                {
                    return BadRequest(new Response<dynamic>(message, "The movie has already been rated by the user.", ""));
                }

                RatedMovie entityObject = new()
                {
                    UserId = userId,
                    MovieId = movieRating.MovieId,
                    Rate = movieRating.Rating,
                    CreatedDate = DateTime.UtcNow
                };

                _context.RatedMovies.Add(entityObject);
                await _context.SaveChangesAsync();

                return Ok(new Response<dynamic>("Successfully rated movie.", entityObject));
            }
            catch (Exception error)
            {
                return BadRequest(new Response<dynamic>(message, error.Message, ""));
            }
        }

        // DELETE
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteRatedMovie(int id)
        {
            string message = "Could not deleted movie.";

            if (id == 0)
            {
                return NotFound(new Response<dynamic>(message, "Empty id.", ""));
            }

            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (identity == null)
            {
                return NotFound(new Response<dynamic>(message, "User not found", ""));
            }

            int userId = Jwt.GetClaimId(identity);

            var ratedMovie = await _context.RatedMovies.Where(x => x.Id == id && x.UserId == userId).FirstOrDefaultAsync();

            if (ratedMovie == null)
            {
                return NotFound(new Response<dynamic>(message, "Movie rating not found", ""));
            }

            _context.RatedMovies.Remove(ratedMovie);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> UserHasRatedMovie(int MovieId)
        {
            bool hasRated = await _context.RatedMovies.AnyAsync(x => x.MovieId == MovieId);
            
            return hasRated;
        }

    }
}
