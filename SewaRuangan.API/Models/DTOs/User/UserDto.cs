using System;
using SewaRuangan.API.Models.Enums;

namespace Sewaruangan.API.Models.DTOs.User
{
    public class UserDto
    {
        public int Id { get; set;}
        public string Name { get; set;} = String.Empty;
        public string Email { get; set;} = String.Empty;
        public string PhoneNumber { get; set;} = String.Empty;
        public UserRole Role { get; set;}
        public string RoleDisplayName => Role.GetDisplayName();
        public bool IsActive { get; set;}
        public DateTime CreatedAt { get; set;}

        // Capabilities (untuk frontend)
        public bool CanApprove => Role.CanApprove();
        public bool CanManageRooms => Role.CanManageRooms();
        public bool CanViewAllReservations => Role.CanViewAllReservations();
        public bool CanManageUsers => Role.CanManageUsers();
        public bool CanDelete => Role.CanDelete();
        
        // Statistics
        public int TotalReservations { get; set; }
    }
}