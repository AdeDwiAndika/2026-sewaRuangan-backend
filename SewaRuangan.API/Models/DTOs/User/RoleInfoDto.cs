using SewaRuangan.API.Models.Enums;

namespace SewaRuangan.API.Models.DTOs.User
{
    public class RoleInfoDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime TokenExpiration { get; set; }
        public UserRole Role { get; set; }
        public string RoleDisplayName { get; set; } = string.Empty;
        
        // Capabilities
        public bool CanApprove { get; set; }
        public bool CanManageRooms { get; set; }
        public bool CanViewAllReservations { get; set; }
        public bool CanManageUsers { get; set; }
    }
}