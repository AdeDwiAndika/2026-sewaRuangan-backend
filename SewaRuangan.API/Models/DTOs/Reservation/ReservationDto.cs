using System;
using SewaRuangan.API.Models.DTOs.User;

namespace SewaRuangan.API.Models.DTOs.Reservation
{
    public class ReservationDto
    {
        public int Id { get; set; }
        public string KodePeminjaman { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public int RuanganId { get; set; }
        public string RuanganNama { get; set; } = string.Empty;
        public string RuanganKode { get; set; } = string.Empty;
        public string Keperluan { get; set; } = string.Empty;
        public int JumlahPeserta { get; set; }
        public DateTime TanggalPeminjaman { get; set; }
        public TimeSpan WaktuMulai { get; set; }
        public TimeSpan WaktuSelesai { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? AdminId { get; set; }
        public string? AdminName { get; set; }
        public DateTime? DisetujuiPada { get; set; }
        public string? CatatanAdmin { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Untuk frontend
        public string StatusDisplayName { get; set; } = string.Empty;
        public string StatusBadgeColor { get; set; } = string.Empty;
        public string TanggalDisplay { get; set; } = string.Empty;
        public string WaktuDisplay { get; set; } = string.Empty;
    }
}