using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SewaRuangan.API.Models.Entities
{
    [Table("StatusHistories")]
    public class StatusHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("reservation_id")]
        public int ReservationId { get; set; }

        [Column("status_sebelum")]
        [StringLength(20)]
        public string StatusSebelum { get; set; }

        [Required]
        [Column("status_sesudah")]
        [StringLength(20)]
        public string StatusSesudah { get; set; }

        [Required]
        [Column("changed_by")]
        public int ChangedBy { get; set; }

        [Column("catatan")]
        public string Catatan { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("ReservationId")]
        public virtual Reservation Reservation { get; set; }

        [ForeignKey("ChangedBy")]
        public virtual User User { get; set; }
    }
}