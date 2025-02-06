using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/favourite")]
    public class FavouriteController : ControllerBase
    {
        private readonly BddContext _context;

        public FavouriteController(BddContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all favourites for the current user.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin, User")]
        public async Task<ActionResult<IEnumerable<Film>>> GetFavourites()
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized("User not found.");
            }

            var favourites = await _context.Favourites
                .Where(f => f.UserId == userId)
                .Include(f => f.Film) // Include Film details
                .Select(f => f.Film)
                .ToListAsync();

            if (!favourites.Any())
            {
                return NotFound("No favourites found.");
            }

            return Ok(favourites);
        }

        /// <summary>
        /// Add a film to the favourites for the current user.
        /// </summary>
        [HttpPost("add")]
        [Authorize(Roles = "Admin, User")]
        public async Task<ActionResult> AddFavourite([FromBody] FavouriteId favouriteId)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized("User not found.");
            }

            // Check if the film exists
            var film = await _context.Films.FindAsync(favouriteId.filmId);
            if (film == null)
            {
                return NotFound($"Film with ID {favouriteId.filmId} not found.");
            }

            // Check if the film is already a favourite
            var existingFavourite = await _context.Favourites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.FilmId == favouriteId.filmId);
            if (existingFavourite != null)
            {
                return Conflict("Film is already in favourites.");
            }

            // Add to favourites
            var favourite = new Favourite
            {
                UserId = userId.Value,
                FilmId = favouriteId.filmId,
                Rating=favouriteId.Rating
            };
            _context.Favourites.Add(favourite);
            await _context.SaveChangesAsync();

            return Ok("Film added to favourites successfully.");
        }

        /// <summary>
        /// Delete a film from the user's favourites.
        /// </summary>
        [HttpDelete("delete")]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> RemoveFavourite([FromBody] FavouriteId favouriteId)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized("User not found.");
            }

            var favourite = await _context.Favourites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.FilmId == favouriteId.filmId);

            if (favourite == null)
            {
                return NotFound($"Film with ID {favouriteId.filmId} is not in favourites.");
            }

            _context.Favourites.Remove(favourite);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        /// <summary>
        /// Helper method to get the user ID from the claims.
        /// </summary>
        private int? GetUserId()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            return userId;
        }
    }

    /// <summary>
    /// Helper model for adding favourites.
    /// </summary>
}
