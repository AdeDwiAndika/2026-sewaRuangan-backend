using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SewaRuangan.API.Data;
using SewaRuangan.API.Models.DTOs;
using SewaRuangan.API.Models.Entities;
using SewaRuangan.API.Models.Enums;

namespace SewaRuangan.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleTestController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RoleTestController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/roletest/roles
        [HttpGet("roles")]
        public IActionResult GetAllRoles()
        {
            var roles = Enum.GetValues(typeof(UserRole))
                .Cast<UserRole>()
                .Select(role => new
                {
                    Role = role.ToString(),
                    DisplayName = role.GetDisplayName(),
                    BadgeColor = role.GetBadgeColor(),
                    Capabilities = new
                    {
                        CanApprove = role.CanApprove(),
                        CanManageRooms = role.CanManageRooms(),
                        CanViewAllReservations = role.CanViewAllReservations(),
                        CanManageUsers = role.CanManageUsers(),
                        CanDelete = role.CanDelete()
                    }
                });

            return Ok(roles);
        }

        // GET: api/roletest/user-capabilities/{userId}
        [HttpGet("user-capabilities/{userId}")]
        public async Task<IActionResult> GetUserCapabilities(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            var capabilities = new
            {
                user.Id,
                user.Name,
                user.Role,
                RoleDisplayName = user.Role.GetDisplayName(),
                BadgeColor = user.Role.GetBadgeColor(),
                CanApprove = user.Role.CanApprove(),
                CanManageRooms = user.Role.CanManageRooms(),
                CanViewAllReservations = user.Role.CanViewAllReservations(),
                CanManageUsers = user.Role.CanManageUsers(),
                CanDelete = user.Role.CanDelete()
            };

            return Ok(capabilities);
        }

        // GET: api/roletest/check-approve/{userId}
        [HttpGet("check-approve/{userId}")]
        public async Task<IActionResult> CheckCanApprove(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            return Ok(new
            {
                user.Id,
                user.Name,
                user.Role,
                CanApprove = user.Role.CanApprove(),
                Message = user.Role.CanApprove() 
                    ? "User ini bisa menyetujui peminjaman" 
                    : "User ini TIDAK bisa menyetujui peminjaman"
            });
        }

        // GET: api/roletest/users-by-role/{role}
        [HttpGet("users-by-role/{role}")]
        public async Task<IActionResult> GetUsersByRole(UserRole role)
        {
            var users = await _context.Users
                .Where(u => u.Role == role && u.IsActive)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    u.Role,
                    RoleDisplayName = role.GetDisplayName()
                })
                .ToListAsync();

            return Ok(new
            {
                Role = role.ToString(),
                DisplayName = role.GetDisplayName(),
                TotalUsers = users.Count,
                Users = users
            });
        }
    }
}