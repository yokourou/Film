using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Models;
using WebApi.Services;
using Microsoft.AspNetCore.Authorization;

namespace WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OmdbController : ControllerBase
    {
        private readonly OmdbService _omdbService;
        private readonly BddContext _context;

        public OmdbController(OmdbService omdbService, BddContext dbContext)
        {
            _omdbService = omdbService ?? throw new ArgumentNullException(nameof(omdbService));
            _context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        // GET: api/Omdb/search/{title}
        [HttpGet("search/{title}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SearchByTitle(string title)
        {
            try
            {
                // Log the roles of the current user
                var userRoles = HttpContext.User.Claims.Where(c => c.Type == "role").Select(c => c.Value);
                Console.WriteLine($"User Roles: {string.Join(", ", userRoles)}");

                if (string.IsNullOrWhiteSpace(title))
                {
                    return BadRequest("Le titre ne peut pas être vide.");
                }

                // Call the OmdbService to search for films by title
                var films = await _omdbService.SearchByTitleAsync(title);

                // Check if the list of films is empty or null
                if (films == null || films.Count == 0)
                {
                    return NotFound($"Aucun film trouvé pour le titre : {title}");
                }

                return Ok(films);
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                Console.WriteLine($"Erreur lors de la recherche des films : {ex.Message}");
                return StatusCode(500, $"Erreur lors de la recherche des films : {ex.Message}");
            }
        }

        // POST: api/Omdb/import
        [HttpPost("import")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ImportFilm([FromBody] Film film)
        {
            try
            {
                // Validate the input
                if (film == null || string.IsNullOrWhiteSpace(film.Imdb))
                {
                    return BadRequest("Le film ou l'ID IMDb est invalide.");
                }

                // Check if the film already exists in the database
                var existingFilm = await _context.Films.FirstOrDefaultAsync(f => f.Imdb == film.Imdb);

                if (existingFilm != null)
                {
                    return Conflict($"Le film avec l'ID IMDb {film.Imdb} existe déjà dans la bibliothèque.");
                }

                // Add the new film to the database
                _context.Films.Add(film);
                await _context.SaveChangesAsync();

                return Ok($"Film '{film.Name}' importé avec succès !");
            }
            catch (DbUpdateException dbEx)
            {
                // Handle database-specific errors
                Console.WriteLine($"Erreur de mise à jour de la base de données : {dbEx.Message}");
                return StatusCode(500, "Erreur de mise à jour de la base de données.");
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                Console.WriteLine($"Erreur lors de l'importation du film : {ex.Message}");
                return StatusCode(500, $"Erreur lors de l'importation du film : {ex.Message}");
            }
        }
        [HttpGet("import/{imdbId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ImportByImdbId(string imdbId)
        {
            if (string.IsNullOrEmpty(imdbId))
                return BadRequest("L'ID IMDB ne peut pas �tre vide.");
            try
            {
                // V�rifie si le film existe d�j� dans la base de donn�es
                List<Film> existingFilm = await _context.Films.Where(f => f.Imdb == imdbId).ToListAsync();
                if (existingFilm.Count>0)
                    return Conflict($"Le film avec l'ID IMDB {imdbId} existe d�j�.");

                // Recherche le film via le service OmdbService 
                var film = await _omdbService.împortByImdbIdAsync(imdbId);

                // Ajoute le film � la base de donn�es
                _context.Films.Add(film);
                await _context.SaveChangesAsync();

                return Ok(film);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors de l'importation du film : {ex.Message}");
            }
        }

    }
}
