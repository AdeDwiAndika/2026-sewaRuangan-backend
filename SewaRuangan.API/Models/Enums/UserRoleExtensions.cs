namespace SewaRuangan.API.Models.Enums
{
    public static class UserRoleExtensions
    {
        // Untuk display di UI
        public static string GetDisplayName(this UserRole role)
        {
            return role switch
            {
                UserRole.Admin => "Administrator",
                UserRole.Staff => "Staff",
                UserRole.Dosen => "Dosen",
                UserRole.Mahasiswa => "Mahasiswa",
                _ => role.ToString()
            };
        }

        // Untuk warna badge di UI
        public static string GetBadgeColor(this UserRole role)
        {
            return role switch
            {
                UserRole.Admin => "danger",      // merah
                UserRole.Staff => "warning",      // kuning
                UserRole.Dosen => "success",      // hijau
                UserRole.Mahasiswa => "primary",  // biru
                _ => "secondary"
            };
        }

        // Cek apakah bisa menyetujui peminjaman
        public static bool CanApprove(this UserRole role)
        {
            return role == UserRole.Admin || role == UserRole.Staff || role == UserRole.Dosen;
        }

        // Cek apakah bisa mengelola ruangan (tambah/edit/hapus)
        public static bool CanManageRooms(this UserRole role)
        {
            return role == UserRole.Admin || role == UserRole.Staff;
        }

        // Cek apakah bisa melihat semua peminjaman
        public static bool CanViewAllReservations(this UserRole role)
        {
            return role == UserRole.Admin || role == UserRole.Staff || role == UserRole.Dosen;
        }

        // Cek apakah bisa mengelola user
        public static bool CanManageUsers(this UserRole role)
        {
            return role == UserRole.Admin;
        }

        // Cek apakah bisa menghapus data
        public static bool CanDelete(this UserRole role)
        {
            return role == UserRole.Admin; // Only admin
        }
    }
}