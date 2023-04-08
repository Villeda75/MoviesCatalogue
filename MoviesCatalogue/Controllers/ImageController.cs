using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MoviesCatalogue.Context;

namespace MoviesCatalogue.Controllers
{
    [Route("images")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ImageController(AppDbContext context, IWebHostEnvironment environment) { 
            _context = context;
            _environment = environment;
        }

        [Authorize(Policy = "AdminPermission")]
        [HttpPost("{id}")]
        [RequestSizeLimit(100_000_000)] // 100 MB limit
        public async Task<IActionResult> Upload(int id, [FromForm] IFormFile file)
        {
            try
            {
                if (id == 0) {
                    return BadRequest("Movie Id is empty.");
                }

                if (file == null || file.Length == 0)
                    return BadRequest("File is empty.");

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

                if (SaveFile(file, fileName).Result)
                {
                    var movie = await _context.Movies.FindAsync(id);

                    if (movie is null)
                    {
                        return NotFound("Movie not found.");
                    }

                    movie.ImagePoster = fileName;

                    _context.Entry(movie).State = EntityState.Modified;

                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        return BadRequest("Could not save image.");
                    }

                    return Ok(new { fileName });
                }

                return BadRequest("File could not saved.");
            }
            catch (Exception error)
            {
                return BadRequest(error.Message);
            }
        }

        [HttpGet("{name}")]
        public IActionResult ObtenerImagen(string name)
        {
            var pathImagen = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Uploads", name);

            if (!System.IO.File.Exists(pathImagen))
            {
                return NotFound();
            }

            var imagenStream = new FileStream(pathImagen, FileMode.Open, FileAccess.Read);
            return new FileStreamResult(imagenStream, "image/jpeg");
        }

        private static async Task<bool> SaveFile(IFormFile file, string name)
        {
            try
            {
                var fileName = name;
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Uploads", fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


    }
}
