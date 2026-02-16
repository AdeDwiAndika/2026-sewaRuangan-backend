using System.ComponentModel.DataAnnotations;

namespace SewaRuangan.API.Models.DTOs.Reservation
{
    public class UpdateReservationDto
    {
        [StringLength(200)]
        public string? Keperluan { get; set; }

        [Range(1, 1000)]
        public int? JumlahPeserta { get; set; }

        [DataType(DataType.Date)]
        public DateTime? TanggalPeminjaman { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan? WaktuMulai { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan? WaktuSelesai { get; set; }
    }
}