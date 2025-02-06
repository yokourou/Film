using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/film")]
    public class FilmController : ControllerBase
    {
        private readonly BddContext _context;

        public FilmController(BddContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Récupère la liste de tous les films.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Film>>> GetFilms()
        {
            var films = await _context.Films.ToListAsync();
            if (films == null || !films.Any())
            {
                return NotFound("Aucun film trouvé.");
            }

            return Ok(films);
        }

        /// <summary>
        /// Recherche des films par mot-clé.
        /// </summary>
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Film>>> SearchFilms([FromQuery] string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                return BadRequest("Le mot-clé est requis.");
            }

            var films = await _context.Films
                .Where(f => f.Name.ToLower().Contains(keyword.ToLower()))
                .ToListAsync();

            if (films == null || !films.Any())
            {
                return NotFound($"Aucun film trouvé pour le mot-clé : {keyword}");
            }

            return Ok(films);
        }

        /// <summary>
        /// Récupère les détails d'un film par son ID.
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Film>> GetFilm(int id)
        {
            var film = await _context.Films.FindAsync(id);

            if (film == null)
            {
                return NotFound($"Film avec l'ID {id} introuvable.");
            }

            return Ok(film);
        }

        /// <summary>
        /// Crée un nouveau film.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Film>> PostFilm(FilmCreation filmCreation)
        {
            if (filmCreation == null)
            {
                return BadRequest("Les données du film sont requises.");
            }

            var film = new Film
            {
                Name = filmCreation.Name,
                Poster = filmCreation.Poster,
                Year = filmCreation.Year,
                Imdb = filmCreation.Imdb,
            };

            _context.Films.Add(film);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFilm), new { id = film.Id }, film);
        }

        /// <summary>
        /// Récupère les informations des films par leurs IDs.
        /// </summary>
        [HttpGet("info")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Film>>> GetFilmsByIds([FromQuery] int[] ids)
        {
            if (ids == null || !ids.Any())
            {
                return BadRequest("Aucun ID de film fourni.");
            }

            var films = await _context.Films
                .Where(f => ids.Contains(f.Id))
                .ToListAsync();

            if (!films.Any())
            {
                return NotFound("Aucun film trouvé pour les IDs fournis.");
            }

            var result = films.Select(f => new
            {
                f.Id,
                f.Name,
                f.Year,
                f.Poster
            }).ToList();

            return Ok(result);
        }

        /// <summary>
        /// Supprime un film par son ID.
        /// </summary>
        [HttpDelete("Delete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteFilm([FromBody] FilmId film_input)
        {
            var film = await _context.Films.FindAsync(film_input.filmId);

            if (film == null)
            {
                Console.WriteLine("id recu est ",film_input.filmId);         
                return NotFound($"Film avec l'ID {film_input.filmId} introuvable.");
            }

            _context.Films.Remove(film);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }


    /// <summary>
    /// Classe utilisée pour la création de films.
    /// </summary>
    public class FilmCreation
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Poster { get; set; }
        public string Imdb { get; set; }
        public string Year { get; set; }
    }
        public class FilmId
    {
        public int filmId {get;set;}
    }

}