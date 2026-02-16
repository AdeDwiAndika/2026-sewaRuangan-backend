using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using SewaRuangan.API.Data;
using SewaRuangan.API.Models.Entities;
using SewaRuangan.API.Models.Enums;
using SewaRuangan.API.Models.DTOs.Reservation;

namespace SewaRuangan.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReservationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. METHOD PALING SPESIFIK - DIATAS
        // GET: api/Reservation/my
        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetMyReservations()
        {
            var userId = GetCurrentUserId();
            var reservations = await _context.Reservations
                .Include(r => r.Ruangan)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .Where(r => r.UserId == userId)
                .ToListAsync();
            var dtos = reservations.Select(r => MapToDto(r)).ToList();
            return Ok(dtos);
        }

        // 2. METHOD DENGAN QUERY PARAMETERS
        // GET: api/Reservation/check-availability?ruanganId=1&tanggal=...
        [HttpGet("check-availability")]
        public async Task<IActionResult> CheckAvailability(
            [FromQuery] int ruanganId,
            [FromQuery] DateTime tanggal,
            [FromQuery] TimeSpan waktuMulai,
            [FromQuery] TimeSpan waktuSelesai)
        {
            var bentrok = await _context.Reservations.AnyAsync(r =>
                r.RuanganId == ruanganId &&
                r.TanggalPeminjaman.Date == tanggal.Date &&
                r.Status != "ditolak" && r.Status != "dibatalkan" &&
                ((waktuMulai >= r.WaktuMulai && waktuMulai < r.WaktuSelesai) ||
                 (waktuSelesai > r.WaktuMulai && waktuSelesai <= r.WaktuSelesai) ||
                 (waktuMulai <= r.WaktuMulai && waktuSelesai >= r.WaktuSelesai)));

            return Ok(new { 
                available = !bentrok,
                message = bentrok ? "Ruangan tidak tersedia" : "Ruangan tersedia"
            });
        }

        // 3. METHOD TANPA PARAMETER
        // GET: api/Reservation
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetReservations()
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            List<Reservation> reservations;
            if (userRole == UserRole.Admin || userRole == UserRole.Staff || userRole == UserRole.Dosen)
            {
                reservations = await _context.Reservations
                    .Include(r => r.User)
                    .Include(r => r.Ruangan)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();
            }
            else
            {
                reservations = await _context.Reservations
                    .Include(r => r.Ruangan)
                    .Include(r => r.User)
                    .Where(r => r.UserId == userId)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();
            }
            var dtos = reservations.Select(r => MapToDto(r)).ToList();
            return Ok(dtos);
        }

        // GET: api/Reservation/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReservationDto>> GetReservation(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Ruangan)
                .Include(r => r.Admin)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
                return NotFound();

            // Cek kepemilikan
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            
            if (reservation.UserId != userId && userRole == UserRole.Mahasiswa)
                return Forbid();

            return Ok(MapToDto(reservation));
        }

        // POST: api/Reservation
        [HttpPost]
        public async Task<ActionResult<ReservationDto>> CreateReservation([FromBody] CreateReservationDto dto)
        {
            try
            {
                // Validasi Waktu
                if (dto.WaktuMulai >= dto.WaktuSelesai)
                {
                    return BadRequest(new { message = "Waktu mulai harus sebelum waktu selesai" });
                }

                // Validasi Ruangan
                var ruangan = await _context.Ruangans.FindAsync(dto.RuanganId);
                if (ruangan == null)
                    return BadRequest(new { message = "Ruangan tidak ditemukan" });
                if (ruangan.Status != "tersedia")
                    return BadRequest(new { message = "Ruangan tidak tersedia untuk dipinjam" });
                

                // Validasi Bentrok Jadwal
                var bentrok = await _context.Reservations.AnyAsync(r =>
                    r.RuanganId == dto.RuanganId &&
                    r.TanggalPeminjaman.Date == dto.TanggalPeminjaman.Date &&
                    r.Status != "ditolak" && r.Status != "dibatalkan" &&
                    ((dto.WaktuMulai >= r.WaktuMulai && dto.WaktuMulai < r.WaktuSelesai) ||
                    (dto.WaktuSelesai > r.WaktuMulai && dto.WaktuSelesai <= r.WaktuSelesai) ||
                    (dto.WaktuMulai <= r.WaktuMulai && dto.WaktuSelesai >= r.WaktuSelesai)));

                if (bentrok)
                    return BadRequest(new { message = "Ruangan sudah dipinjam di jam tersebut" });
                
                // Validasi Kapasitas
                if (dto.JumlahPeserta > ruangan.Kapasitas)
                    return BadRequest(new { message = $"Jumlah peserta melebihi kapasitas ruangan ({ruangan.Kapasitas})" });

                // Buat entity Reservation dari DTO
                var reservation = new Reservation
                {
                    RuanganId = dto.RuanganId,
                    Keperluan = dto.Keperluan,
                    JumlahPeserta = dto.JumlahPeserta,
                    TanggalPeminjaman = dto.TanggalPeminjaman,
                    WaktuMulai = dto.WaktuMulai,
                    WaktuSelesai = dto.WaktuSelesai,
                    UserId = GetCurrentUserId(),
                    KodePeminjaman = $"PMJ-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}",
                    Status = "menunggu",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Reservations.Add(reservation);
                await _context.SaveChangesAsync();

                // Ambil data lengkap untuk response
                var createdReservation = await _context.Reservations
                    .Include(r => r.User)
                    .Include(r => r.Ruangan)
                    .FirstOrDefaultAsync(r => r.Id == reservation.Id);

                var dtoResult = MapToDto(createdReservation!);
                return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, dtoResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }
        // Helper method untuk mapping entity ke DTO
        private ReservationDto MapToDto(Reservation r)
        {
            return new ReservationDto
            {
                Id = r.Id,
                KodePeminjaman = r.KodePeminjaman,
                UserId = r.UserId,
                UserName = r.User?.Name ?? string.Empty,
                UserEmail = r.User?.Email ?? string.Empty,
                RuanganId = r.RuanganId,
                RuanganNama = r.Ruangan?.NamaRuangan ?? string.Empty,
                RuanganKode = r.Ruangan?.KodeRuangan ?? string.Empty,
                Keperluan = r.Keperluan,
                JumlahPeserta = r.JumlahPeserta,
                TanggalPeminjaman = r.TanggalPeminjaman,
                WaktuMulai = r.WaktuMulai,
                WaktuSelesai = r.WaktuSelesai,
                Status = r.Status,
                AdminId = r.AdminId,
                AdminName = r.Admin?.Name,
                DisetujuiPada = r.DisetujuiPada,
                CatatanAdmin = r.CatatanAdmin,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                StatusDisplayName = r.Status, // bisa diubah sesuai kebutuhan
                StatusBadgeColor = string.Empty, // bisa diisi sesuai kebutuhan
                TanggalDisplay = r.TanggalPeminjaman.ToString("dd/MM/yyyy"),
                WaktuDisplay = $"{r.WaktuMulai:hh\\:mm} - {r.WaktuSelesai:hh\\:mm}"
            };
        }

        // PUT: api/Reservation/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReservation(int id, [FromBody] UpdateReservationDto dto)
        {
            var existing = await _context.Reservations.FindAsync(id);
            if (existing == null)
                return NotFound();

            // Cek kepemilikan
            var userId = GetCurrentUserId();
            if (existing.UserId != userId)
                return Forbid();

            // Ambil ruangan terkait
            var ruangan = await _context.Ruangans.FindAsync(existing.RuanganId);
            if (ruangan == null)
                return BadRequest(new { message = "Ruangan tidak ditemukan" });

            // Validasi waktu
            var waktuMulai = dto.WaktuMulai ?? existing.WaktuMulai;
            var waktuSelesai = dto.WaktuSelesai ?? existing.WaktuSelesai;
            if (waktuMulai >= waktuSelesai)
                return BadRequest(new { message = "Waktu mulai harus sebelum waktu selesai" });

            // Validasi bentrok jadwal
            var tanggal = dto.TanggalPeminjaman ?? existing.TanggalPeminjaman;
            var bentrok = await _context.Reservations.AnyAsync(r =>
                r.Id != id &&
                r.RuanganId == existing.RuanganId &&
                r.TanggalPeminjaman.Date == tanggal.Date &&
                r.Status != "ditolak" && r.Status != "dibatalkan" &&
                ((waktuMulai >= r.WaktuMulai && waktuMulai < r.WaktuSelesai) ||
                (waktuSelesai > r.WaktuMulai && waktuSelesai <= r.WaktuSelesai) ||
                (waktuMulai <= r.WaktuMulai && waktuSelesai >= r.WaktuSelesai))
            );
            if (bentrok)
                return BadRequest(new { message = "Ruangan sudah dipinjam di jam tersebut" });

            // Validasi kapasitas
            var jumlahPeserta = dto.JumlahPeserta ?? existing.JumlahPeserta;
            if (jumlahPeserta > ruangan.Kapasitas)
                return BadRequest(new { message = $"Jumlah peserta melebihi kapasitas ruangan ({ruangan.Kapasitas})" });

            // Hanya bisa update jika masih menunggu
            if (existing.Status != "menunggu")
                return BadRequest(new { message = "Hanya peminjaman dengan status menunggu yang bisa diubah" });

            // Update field yang boleh diubah
            if (dto.Keperluan != null)
                existing.Keperluan = dto.Keperluan;
            if (dto.JumlahPeserta.HasValue)
                existing.JumlahPeserta = dto.JumlahPeserta.Value;
            existing.UpdatedAt = DateTime.UtcNow;
            if (dto.TanggalPeminjaman.HasValue)
                existing.TanggalPeminjaman = dto.TanggalPeminjaman.Value;
            if (dto.WaktuMulai.HasValue)
                existing.WaktuMulai = dto.WaktuMulai.Value;
            if (dto.WaktuSelesai.HasValue)
                existing.WaktuSelesai = dto.WaktuSelesai.Value;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Peminjaman berhasil diubah" });
        }
        
        // DELETE: api/Reservation/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
                return NotFound();

            // Hanya admin yang bisa hapus
            var userRole = GetCurrentUserRole();
            if (userRole != UserRole.Admin)
                return Forbid();

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Peminjaman berhasil dihapus" });
        }

        // POST: api/Reservation/5/approve
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApproveReservation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
                return NotFound();

            // Cek role (Admin, Staff, Dosen bisa approve)
            var userRole = GetCurrentUserRole();
            if (userRole != UserRole.Admin && userRole != UserRole.Staff && userRole != UserRole.Dosen)
                return Forbid();


            // Only allow approve if not already approved or cancelled/rejected
            if (reservation.Status == "disetujui" || reservation.Status == "dibatalkan" || reservation.Status == "ditolak")
                return BadRequest(new { message = "Peminjaman sudah tidak dapat disetujui" });

            reservation.Status = "disetujui";
            reservation.AdminId = GetCurrentUserId();
            reservation.DisetujuiPada = DateTime.UtcNow;
            reservation.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Peminjaman disetujui" });
        }

        // POST: api/Reservation/5/reject
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> RejectReservation(int id, [FromBody] string catatan)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
                return NotFound();

            // Cek role (Admin, Staff, Dosen bisa reject)
            var userRole = GetCurrentUserRole();
            if (userRole != UserRole.Admin && userRole != UserRole.Staff && userRole != UserRole.Dosen)
                return Forbid();


            // Allow rejection if status is 'menunggu' or 'disetujui' (approved but needs to be rejected due to issue)
            if (reservation.Status != "menunggu" && reservation.Status != "disetujui")
                return BadRequest(new { message = "Hanya peminjaman dengan status menunggu atau disetujui yang bisa ditolak" });

            reservation.Status = "ditolak";
            reservation.AdminId = GetCurrentUserId();
            reservation.CatatanAdmin = catatan;
            reservation.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Peminjaman ditolak" });
        }

        // POST: api/Reservation/5/cancel
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelReservation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
                return NotFound();

            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            // Pemilik atau admin bisa cancel
            if (reservation.UserId != userId && userRole != UserRole.Admin)
                return Forbid();

            if (reservation.Status != "menunggu" && reservation.Status != "disetujui")
                return BadRequest(new { message = "Hanya peminjaman dengan status menunggu/disetujui yang bisa dibatalkan" });

            reservation.Status = "dibatalkan";
            reservation.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Peminjaman dibatalkan" });
        }

        // Helper methods
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("user_id")?.Value 
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;
            
            if (userIdClaim == null)
                return 0;
                
            return int.Parse(userIdClaim);
        }

        private UserRole GetCurrentUserRole()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            return Enum.Parse<UserRole>(roleClaim ?? "Mahasiswa");
        }
    }
}