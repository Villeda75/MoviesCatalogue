﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
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

        // GET: api/Movies
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Movie>>> GetMovies()
        {
            //string? search, string category, int releasedYear
            try
            {
                List<Movie> result = await _memoryCache.GetOrCreateAsync("allmovies", async entry =>
                {
                    var movies = await _context.Movies.ToListAsync();

                    return movies;
                });

                return result;

            }
            catch (Exception error)
            {
                string err = error.Message;
                throw;
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
