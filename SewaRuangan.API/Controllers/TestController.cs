using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SewaRuangan.API.Data;

namespace SewaRuangan.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TestController> _logger;
        private readonly IWebHostEnvironment _environment; // Tambahkan ini

        // Update constructor
        public TestController(
            ApplicationDbContext context, 
            ILogger<TestController> logger,
            IWebHostEnvironment environment) // Tambahkan parameter ini
        {
            _context = context;
            _logger = logger;
            _environment = environment; // Simpan environment
        }

        /// <summary>
        /// Test koneksi database
        /// </summary>
        [HttpGet("database")]
        public async Task<IActionResult> TestDatabaseConnection()
        {
            try
            {
                _logger.LogInformation("Mencoba koneksi ke PostgreSQL...");
                
                var canConnect = await _context.Database.CanConnectAsync();
                var databaseName = _context.Database.GetDbConnection().Database;
                
                return Ok(new
                {
                    success = true,
                    message = "✅ Koneksi database berhasil!",
                    database = new
                    {
                        name = databaseName,
                        canConnect,
                        provider = _context.Database.ProviderName
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Koneksi database gagal");
                return StatusCode(500, new
                {
                    success = false,
                    message = "❌ Koneksi database gagal!",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        /// <summary>
        /// Info API dan database (TANPA referensi ke 'app')
        /// </summary>
        [HttpGet("info")]
        public IActionResult GetInfo()
        {
            return Ok(new
            {
                status = "API is running",
                timestamp = DateTime.UtcNow,
                environment = _environment.EnvironmentName, // ✅ Pakai _environment, bukan app
                machineName = Environment.MachineName,
                osVersion = Environment.OSVersion.VersionString,
                dotNetVersion = Environment.Version.ToString(),
                assembly = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0",
                database = new
                {
                    provider = _context.Database.ProviderName,
                    connectionString = MaskConnectionString(_context.Database.GetConnectionString())
                }
            });
        }

        /// <summary>
        /// Cek tabel-tabel yang ada
        /// </summary>
        [HttpGet("tables")]
        public async Task<IActionResult> GetTables()
        {
            try
            {
                // Cek apakah koneksi database bisa
                var canConnect = await _context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Tidak dapat terhubung ke database"
                    });
                }

                // Cek apakah tabel-tabel sudah dibuat
                var userExists = await _context.Users.AnyAsync();
                var ruanganExists = await _context.Ruangans.AnyAsync();
                var reservationExists = await _context.Reservations.AnyAsync();
                var historyExists = await _context.StatusHistories.AnyAsync();

                // Hitung jumlah record (akan 0 karena belum ada data)
                var userCount = await _context.Users.CountAsync();
                var ruanganCount = await _context.Ruangans.CountAsync();

                return Ok(new
                {
                    success = true,
                    tables = new
                    {
                        users = new { exists = true, count = userCount },
                        ruangans = new { exists = true, count = ruanganCount },
                        reservations = new { exists = reservationExists, count = await _context.Reservations.CountAsync() },
                        statusHistories = new { exists = historyExists, count = await _context.StatusHistories.CountAsync() }
                    },
                    message = "Tabel berhasil dibuat! Database siap digunakan."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal membaca tabel");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Gagal membaca tabel",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Test endpoint sederhana
        /// </summary>
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new
            {
                message = "pong",
                timestamp = DateTime.UtcNow,
                serverTime = DateTime.Now.ToString("HH:mm:ss")
            });
        }

        /// <summary>
        /// Cek koneksi database sederhana
        /// </summary>
        [HttpGet("quick-test")]
        public async Task<IActionResult> QuickTest()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                
                return Ok(new
                {
                    canConnect,
                    message = canConnect ? "Database OK" : "Database Error"
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    canConnect = false,
                    error = ex.Message
                });
            }
        }

        private string MaskConnectionString(string? connString)
        {
            if (string.IsNullOrEmpty(connString))
                return "No connection string";

            // Sembunyikan password
            var masked = connString;
            if (masked.Contains("Password="))
            {
                var parts = masked.Split(new[] { "Password=" }, StringSplitOptions.None);
                if (parts.Length > 1)
                {
                    var afterPassword = parts[1].Split(';');
                    masked = parts[0] + "Password=***;" + string.Join(";", afterPassword.Skip(1));
                }
            }
            
            // Sembunyikan username juga
            if (masked.Contains("Username="))
            {
                masked = masked.Replace("Username=postgres", "Username=***");
            }
            
            return masked;
        }
    }
}