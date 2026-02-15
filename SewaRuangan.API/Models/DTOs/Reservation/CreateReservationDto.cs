using System;
using System.ComponentModel.DataAnnotations;

namespace SewaRuangan.API.Models.DTOs.Reservation
{
    public class CreateReservationDto
    {
        [Required]
        public int RuanganId { get; set; }

        [Required]
        [StringLength(200)]
        public string Keperluan { get; set; } = string.Empty;

        [Required]
        [Range(1, 1000)]
        public int JumlahPeserta { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime TanggalPeminjaman { get; set; }

        [Required]
        public TimeSpan WaktuMulai { get; set; }

        [Required]
        public TimeSpan WaktuSelesai { get; set; }
    }
}