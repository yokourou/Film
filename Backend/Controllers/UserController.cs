using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly BddContext _context;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly JwtService _jwtService;

        public UserController(BddContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
            _passwordHasher = new PasswordHasher<User>();
        }

        // GET: api/user/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound("Utilisateur non trouvé.");
            }

            return Ok(user);
        }

        public class UserCreation
        {
            public string Pseudo { get; set; }
            public string Password { get; set; }
            public Role Role { get; set; }
        }
        /* */
        [HttpGet("getUsers")]
        [Authorize(Roles = "Admin,User")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
        var users = await _context.Users.Select(u => new 
        {
            Id = u.Id,
            Pseudo = u.Pseudo, // Assurez-vous que `UserName` est le champ pour le pseudo dans votre modèle.
            Role = u.Role  // Assurez-vous que `UserRole` est le champ pour le rôle dans votre modèle.
        })
        .ToListAsync();
            if (users == null || !users.Any())
            {
                return NotFound("Aucun film trouvé.");
            }
            return Ok(users);
        }
        // POST: api/user/register
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<User>> Register(UserCreation userCreation)
        {
            if (string.IsNullOrWhiteSpace(userCreation.Pseudo) || string.IsNullOrWhiteSpace(userCreation.Password))
            {
                return BadRequest("Pseudo et mot de passe sont requis.");
            }

            var existingUser = await _context.Users.AnyAsync(u => u.Pseudo == userCreation.Pseudo);
            if (existingUser)
            {
                return Conflict("Un utilisateur avec ce pseudo existe déjà.");
            }

            var user = new User
            {
                Pseudo = userCreation.Pseudo,
                Role = userCreation.Role,
                Password = _passwordHasher.HashPassword(null, userCreation.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        // PUT: api/user/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> UpdateUser(int id, User userUpdate)
        {
            if (id != userUpdate.Id)
            {
                return BadRequest("L'ID de l'utilisateur ne correspond pas.");
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound("Utilisateur non trouvé.");
            }

            user.Pseudo = userUpdate.Pseudo;
            if (!string.IsNullOrWhiteSpace(userUpdate.Password))
            {
                user.Password = _passwordHasher.HashPassword(null, userUpdate.Password);
            }
            user.Role = userUpdate.Role;

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, "Erreur de concurrence lors de la mise à jour.");
            }

            return Ok(user);
        }

        // DELETE: api/user/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound("Utilisateur non trouvé.");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/user/login
        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login(UserInfo request)
        {
            if (string.IsNullOrWhiteSpace(request.Pseudo) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Pseudo et mot de passe sont requis.");
            }

            var user = _context.Users.FirstOrDefault(u => u.Pseudo == request.Pseudo);
            if (user == null)
            {
                return Unauthorized("Pseudo ou mot de passe incorrect.");
            }

            var result = _passwordHasher.VerifyHashedPassword(null, user.Password, request.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                return Unauthorized("Pseudo ou mot de passe incorrect.");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Pseudo),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var token = _jwtService.GenerateToken(claims);

            return Ok(new
            {
                User = new
                {
                    user.Id,
                    user.Pseudo,
                    user.Role
                },
                Token = token
            });
        }

        // GET: api/user
        [HttpGet]
        [Authorize]
        public IActionResult GetUserDetails()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(new
            {
                Id = userId,
                Name = userName,
                Role = userRole
            });
        }
    }
}
