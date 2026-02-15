namespace SewaRuangan.API.Models.DTOs.Reservation
{
    public class RejectReservationDto
    {
        public int AdminId { get; set; }
        public string Catatan { get; set; } = string.Empty;
    }
}