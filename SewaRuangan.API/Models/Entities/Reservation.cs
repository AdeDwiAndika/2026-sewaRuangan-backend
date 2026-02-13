using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SewaRuangan.API.Models.Entities
{
    [Table("Reservations")]
    public class Reservation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        [Column("kode_peminjaman")]
        public string KodePeminjaman { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("ruangan_id")]
        public int RuanganId { get; set; }

        [Required]
        [StringLength(200)]
        public string Keperluan { get; set; }

        [Required]
        [Column("jumlah_peserta")]
        public int JumlahPeserta { get; set; }

        [Required]
        [Column("tanggal_peminjaman", TypeName = "date")]
        public DateTime TanggalPeminjaman { get; set; }

        [Required]
        [Column("waktu_mulai", TypeName = "time")]
        public TimeSpan WaktuMulai { get; set; }

        [Required]
        [Column("waktu_selesai", TypeName = "time")]
        public TimeSpan WaktuSelesai { get; set; }

        [Required]
        [StringLength(20)]
        [Column("status")]
        public string Status { get; set; } = "menunggu"; // menunggu, disetujui, ditolak, dibatalkan, selesai

        [Column("admin_id")]
        public int? AdminId { get; set; }

        [Column("disetujui_pada")]
        public DateTime? DisetujuiPada { get; set; }

        [Column("catatan_admin")]
        public string CatatanAdmin { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("RuanganId")]
        public virtual Ruangan Ruangan { get; set; }

        [ForeignKey("AdminId")]
        public virtual User Admin { get; set; }

        public virtual ICollection<StatusHistory> StatusHistories { get; set; }
    }
}