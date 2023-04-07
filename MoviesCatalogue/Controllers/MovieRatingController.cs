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

        // GET: api/MovieRating/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RatedMovie>> GetRatedMovie(int id)
        {
          if (_context.RatedMovies == null)
          {
              return NotFound();
          }
            var ratedMovie = await _context.RatedMovies.FindAsync(id);

            if (ratedMovie == null)
            {
                return NotFound();
            }

            return ratedMovie;
        }

        //POST
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<RatedMovie>> PostRatedMovie(RatedMovie ratedMovie)
        {
          if (_context.RatedMovies == null)
          {
              return Problem("Entity set 'AppDbContext.RatedMovies'  is null.");
          }
            _context.RatedMovies.Add(ratedMovie);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRatedMovie", new { id = ratedMovie.Id }, ratedMovie);
        }

        // DELETE
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteRatedMovie(int id)
        {
            if (_context.RatedMovies == null)
            {
                return NotFound();
            }
            var ratedMovie = await _context.RatedMovies.FindAsync(id);
            if (ratedMovie == null)
            {
                return NotFound();
            }

            _context.RatedMovies.Remove(ratedMovie);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RatedMovieExists(int id)
        {
            return (_context.RatedMovies?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
