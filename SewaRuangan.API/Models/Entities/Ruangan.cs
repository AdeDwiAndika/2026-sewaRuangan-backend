using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SewaRuangan.API.Models.Entities
{
    [Table("Ruangans")]
    public class Ruangan
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        [Column("kode_ruangan")]
        public string KodeRuangan { get; set; }

        [Required]
        [StringLength(100)]
        [Column("nama_ruangan")]
        public string NamaRuangan { get; set; }

        [Required]
        [StringLength(50)]
        public string Gedung { get; set; }

        [Required]
        public int Lantai { get; set; }

        [Required]
        public int Kapasitas { get; set; }

        public string Fasilitas { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "tersedia"; // tersedia, dipelihara, tidak_aktif

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual ICollection<Reservation> Reservations { get; set; }
    }
}