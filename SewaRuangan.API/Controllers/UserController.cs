using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using SewaRuangan.API.Data;
using Sewaruangan.API.Models.DTOs.User;
using SewaRuangan.API.Models.Entities;
using SewaRuangan.API.Models.Enums;
using SewaRuangan.API.Helpers;
using System.Security.Cryptography;
using System.Text;
using System.Security.Claims;
using SewaRuangan.API.Models.DTOs.User;

namespace SewaRuangan.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtHelper _jwtHelper;

        public UserController(ApplicationDbContext context, JwtHelper jwtHelper)
        {
            _context = context;
            _jwtHelper = jwtHelper;
        }

        // POST: api/user/register
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(CreateUserDto createUserDto)
        {
            // Cek email sudah ada
            if (await _context.Users.AnyAsync(u => u.Email == createUserDto.Email))
            {
                return BadRequest(new { message = "Email sudah terdaftar" });
            }

            var user = new User
            {
                Name = createUserDto.Name,
                Email = createUserDto.Email,
                PhoneNumber = createUserDto.PhoneNumber,
                Role = createUserDto.Role,
                PasswordHash = HashPassword(createUserDto.Password),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var response = MapToUserDto(user);
            return Ok(response);
        }

        // POST: api/user/login
        [HttpPost("login")]
        public async Task<ActionResult<RoleInfoDto>> Login(LoginDto loginDto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null || !user.IsActive)
            {
                return Unauthorized(new { message = "Email atau password salah" });
            }

            if (!VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Email atau password salah" });
            }

            // Update last login
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Generate JWT Token
            var token = _jwtHelper.GenerateToken(user);
            var tokenExpiration = DateTime.Now.AddMinutes(Convert.ToDouble(GetConfigValue("Jwt:DurationInMinutes", "60")));

            var response = new RoleInfoDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Token = token,
                TokenExpiration = tokenExpiration,
                Role = user.Role,
                RoleDisplayName = user.Role.GetDisplayName(),
                CanApprove = user.Role.CanApprove(),
                CanManageRooms = user.Role.CanManageRooms(),
                CanViewAllReservations = user.Role.CanViewAllReservations(),
                CanManageUsers = user.Role.CanManageUsers()
            };

            return Ok(response);
        }

        // GET: api/user/me (get current user profile)
        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var userId = GetCurrentUserId();
            var user = await _context.Users
                .Include(u => u.Reservations)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound(new { message = "User tidak ditemukan" });

            return Ok(MapToUserDto(user));
        }

        // GET: api/user
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            // Hanya Admin yang bisa lihat semua users
            if (!IsAdmin())
            {
                return Forbid();
            }

            var users = await _context.Users
                .Include(u => u.Reservations)
                .ToListAsync();

            return Ok(users.Select(u => MapToUserDto(u)));
        }

        // GET: api/user/{id}
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            // User bisa lihat dirinya sendiri, admin bisa lihat semua
            if (currentUserId != id && !IsAdmin())
            {
                return Forbid();
            }

            var user = await _context.Users
                .Include(u => u.Reservations)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound();

            return Ok(MapToUserDto(user));
        }

        // GET: api/user/role/{role}
        [Authorize]
        [HttpGet("role/{role}")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsersByRole(UserRole role)
        {
            // Hanya Admin yang bisa filter by role
            if (!IsAdmin())
            {
                return Forbid();
            }

            var users = await _context.Users
                .Where(u => u.Role == role && u.IsActive)
                .Include(u => u.Reservations)
                .ToListAsync();

            return Ok(users.Select(u => MapToUserDto(u)));
        }

        // PUT: api/user/{id}/activate
        [Authorize]
        [HttpPut("{id}/activate")]
        public async Task<IActionResult> ActivateUser(int id)
        {
            if (!IsAdmin())
            {
                return Forbid();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "User diaktifkan" });
        }

        // PUT: api/user/{id}/deactivate
        [Authorize]
        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> DeactivateUser(int id)
        {
            if (!IsAdmin())
            {
                return Forbid();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "User dinonaktifkan" });
        }

        // Helper methods
        private UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                TotalReservations = user.Reservations?.Count ?? 0
                // RoleDisplayName, CanApprove, dll sudah otomatis dari property getter
            };
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private bool VerifyPassword(string password, string passwordHash)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == passwordHash;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("user_id")?.Value 
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;
            
            return int.Parse(userIdClaim ?? "0");
        }

        private UserRole GetCurrentUserRole()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            return Enum.Parse<UserRole>(roleClaim ?? "Mahasiswa");
        }

        private bool IsAdmin()
        {
            return GetCurrentUserRole() == UserRole.Admin;
        }

        private string GetConfigValue(string key, string defaultValue)
        {
            var config = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            return config[key] ?? defaultValue;
        }
    }

    // DTOs untuk request (tetap perlu dibuat)
    public class CreateUserDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Mahasiswa;
    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}