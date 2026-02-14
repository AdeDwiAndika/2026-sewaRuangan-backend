using SewaRuangan.API.Models.Enums;

namespace Sewaruangan.API.Models.DTOs.User
{
    public class CreateUserDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Mahasiswa;
    }
}