using System.ComponentModel.DataAnnotations;

namespace SewaRuangan.API.Models.DTOs.Reservation
{
    public class UpdateReservationDto
    {
        [StringLength(200)]
        public string? Keperluan { get; set; }

        [Range(1, 1000)]
        public int? JumlahPeserta { get; set; }
    }
}