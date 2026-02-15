using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SewaRuangan.API.Data;
using SewaRuangan.API.Models.Entities;

namespace SewaRuangan.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RuanganController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RuanganController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Ruangan
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ruangan>>> GetRuangans()
        {
            return await _context.Ruangans
                .OrderBy(r => r.Gedung)
                .ThenBy(r => r.Lantai)
                .ToListAsync();
        }

        // GET: api/Ruangan/tersedia
        [HttpGet("tersedia")]
        public async Task<ActionResult<IEnumerable<Ruangan>>> GetRuanganTersedia()
        {
            return await _context.Ruangans
                .Where(r => r.Status == "tersedia")
                .OrderBy(r => r.Gedung)
                .ThenBy(r => r.Lantai)
                .ToListAsync();
        }

        // GET: api/Ruangan/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Ruangan>> GetRuangan(int id)
        {
            var ruangan = await _context.Ruangans.FindAsync(id);

            if (ruangan == null)
                return NotFound(new { message = "Ruangan tidak ditemukan" });

            return ruangan;
        }

        // GET: api/Ruangan/gedung/Gedung Rektorat
        [HttpGet("gedung/{gedung}")]
        public async Task<ActionResult<IEnumerable<Ruangan>>> GetRuanganByGedung(string gedung)
        {
            return await _context.Ruangans
                .Where(r => r.Gedung == gedung)
                .OrderBy(r => r.Lantai)
                .ToListAsync();
        }

        // POST: api/Ruangan (khusus admin)
        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<ActionResult<Ruangan>> CreateRuangan(Ruangan ruangan)
        {
            // Cek kode ruangan unik
            var existing = await _context.Ruangans
                .AnyAsync(r => r.KodeRuangan == ruangan.KodeRuangan);
            
            if (existing)
                return BadRequest(new { message = "Kode ruangan sudah ada" });

            ruangan.CreatedAt = DateTime.UtcNow;
            ruangan.UpdatedAt = DateTime.UtcNow;

            _context.Ruangans.Add(ruangan);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRuangan), new { id = ruangan.Id }, ruangan);
        }

        // PUT: api/Ruangan/5 (khusus admin)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateRuangan(int id, Ruangan ruangan)
        {
            if (id != ruangan.Id)
                return BadRequest();

            var existing = await _context.Ruangans.FindAsync(id);
            if (existing == null)
                return NotFound();

            // Update field
            existing.NamaRuangan = ruangan.NamaRuangan;
            existing.Gedung = ruangan.Gedung;
            existing.Lantai = ruangan.Lantai;
            existing.Kapasitas = ruangan.Kapasitas;
            existing.Fasilitas = ruangan.Fasilitas;
            existing.Status = ruangan.Status;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Ruangan berhasil diupdate" });
        }

        // DELETE: api/Ruangan/5 (khusus admin)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteRuangan(int id)
        {
            var ruangan = await _context.Ruangans.FindAsync(id);
            if (ruangan == null)
                return NotFound();

            // Cek apakah ada reservasi aktif
            var hasActiveReservations = await _context.Reservations
                .AnyAsync(r => r.RuanganId == id && 
                    (r.Status == "menunggu" || r.Status == "disetujui"));

            if (hasActiveReservations)
                return BadRequest(new { message = "Tidak bisa menghapus ruangan yang memiliki peminjaman aktif" });

            _context.Ruangans.Remove(ruangan);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Ruangan berhasil dihapus" });
        }
    }
}