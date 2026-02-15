using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SewaRuangan.API.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SewaRuangan.API.Data.Seed
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

            try
            {
                // Cek apakah sudah ada data ruangan
                if (context.Ruangans.Any())
                {
                    logger.LogInformation("Database already seeded");
                    return;
                }

                logger.LogInformation("Starting database seeding for Ruangan...");

                var ruangans = SeedRuangans();
                await context.Ruangans.AddRangeAsync(ruangans);
                await context.SaveChangesAsync();

                logger.LogInformation($"✅ {ruangans.Count} ruangans seeded successfully!");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database");
                throw;
            }
        }

        private static List<Ruangan> SeedRuangans()
        {
            return new List<Ruangan>
            {
                new Ruangan
                {
                    KodeRuangan = "R001",
                    NamaRuangan = "Ruang Seminar A",
                    Gedung = "Gedung Rektorat",
                    Lantai = 2,
                    Kapasitas = 50,
                    Fasilitas = "Proyektor, AC, Whiteboard, Sound System",
                    Status = "tersedia",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Ruangan
                {
                    KodeRuangan = "R002",
                    NamaRuangan = "Ruang Seminar B",
                    Gedung = "Gedung Rektorat",
                    Lantai = 2,
                    Kapasitas = 30,
                    Fasilitas = "Proyektor, AC, Whiteboard, TV",
                    Status = "tersedia",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Ruangan
                {
                    KodeRuangan = "LAB01",
                    NamaRuangan = "Lab Komputer 1",
                    Gedung = "Gedung Fakultas Teknik",
                    Lantai = 3,
                    Kapasitas = 40,
                    Fasilitas = "Komputer, Proyektor, AC, Whiteboard",
                    Status = "tersedia",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Ruangan
                {
                    KodeRuangan = "LAB02",
                    NamaRuangan = "Lab Komputer 2",
                    Gedung = "Gedung Fakultas Teknik",
                    Lantai = 3,
                    Kapasitas = 40,
                    Fasilitas = "Komputer, Proyektor, AC, Whiteboard",
                    Status = "dipelihara",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Ruangan
                {
                    KodeRuangan = "R101",
                    NamaRuangan = "Ruang Kelas 101",
                    Gedung = "Gedung Perkuliahan A",
                    Lantai = 1,
                    Kapasitas = 60,
                    Fasilitas = "Proyektor, AC, Whiteboard",
                    Status = "tersedia",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Ruangan
                {
                    KodeRuangan = "R102",
                    NamaRuangan = "Ruang Kelas 102",
                    Gedung = "Gedung Perkuliahan A",
                    Lantai = 1,
                    Kapasitas = 60,
                    Fasilitas = "Proyektor, AC, Whiteboard",
                    Status = "tersedia",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Ruangan
                {
                    KodeRuangan = "AULA",
                    NamaRuangan = "Aula Kampus",
                    Gedung = "Gedung Serbaguna",
                    Lantai = 1,
                    Kapasitas = 200,
                    Fasilitas = "Proyektor, AC, Sound System, Panggung",
                    Status = "tersedia",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Ruangan
                {
                    KodeRuangan = "R201",
                    NamaRuangan = "Ruang Rapat Senat",
                    Gedung = "Gedung Rektorat",
                    Lantai = 3,
                    Kapasitas = 20,
                    Fasilitas = "Proyektor, AC, Whiteboard, Meja Rapat",
                    Status = "tersedia",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Ruangan
                {
                    KodeRuangan = "GOR",
                    NamaRuangan = "GOR Olahraga",
                    Gedung = "Kompleks Olahraga",
                    Lantai = 1,
                    Kapasitas = 500,
                    Fasilitas = "Lapangan Basket, Sound System, Tribun",
                    Status = "tersedia",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Ruangan
                {
                    KodeRuangan = "PERPUS",
                    NamaRuangan = "Ruang Diskusi Perpustakaan",
                    Gedung = "Perpustakaan Pusat",
                    Lantai = 2,
                    Kapasitas = 15,
                    Fasilitas = "TV, Whiteboard, AC, Sofa",
                    Status = "tersedia",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };
        }
    }
}