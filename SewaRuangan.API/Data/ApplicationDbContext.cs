using Microsoft.EntityFrameworkCore;
using SewaRuangan.API.Models.Entities;
using SewaRuangan.API.Models.Enums;

namespace SewaRuangan.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Ruangan> Ruangans { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<StatusHistory> StatusHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.Id);
                
                // Indexes
                entity.HasIndex(u => u.Email).IsUnique();
                
                // Properties
                entity.Property(u => u.Role)
                    .HasConversion<int>()  // Simpan enum sebagai integer di database
                    .HasDefaultValue(UserRole.Mahasiswa);
                    
                entity.Property(u => u.IsActive).HasDefaultValue(true);
                
                // Timestamps - menggunakan NOW() untuk PostgreSQL
                entity.Property(u => u.CreatedAt)
                    .HasDefaultValueSql("NOW()");
                    
                entity.Property(u => u.UpdatedAt)
                    .HasDefaultValueSql("NOW()");
                
                // Relationships (akan dikonfigurasi nanti)
            });

            // Configure Ruangan
            modelBuilder.Entity<Ruangan>(entity =>
            {
                entity.ToTable("Ruangans");
                entity.HasKey(e => e.Id);
                
                entity.HasIndex(r => r.KodeRuangan).IsUnique();
                
                entity.Property(r => r.Status).HasDefaultValue("tersedia");
                entity.Property(r => r.CreatedAt).HasDefaultValueSql("NOW()");
                entity.Property(r => r.UpdatedAt).HasDefaultValueSql("NOW()");
            });

            // Configure Reservation
            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.ToTable("Reservations");
                entity.HasKey(e => e.Id);
                
                entity.HasIndex(r => r.KodePeminjaman).IsUnique();
                
                entity.Property(r => r.Status).HasDefaultValue("menunggu");
                entity.Property(r => r.CreatedAt).HasDefaultValueSql("NOW()");
                entity.Property(r => r.UpdatedAt).HasDefaultValueSql("NOW()");
                
                // Check constraint: waktu selesai harus setelah waktu mulai
                entity.HasCheckConstraint("CK_Reservation_Time", "\"waktu_selesai\" > \"waktu_mulai\"");
            });

            // Configure StatusHistory
            modelBuilder.Entity<StatusHistory>(entity =>
            {
                entity.ToTable("StatusHistories");
                entity.HasKey(e => e.Id);
                
                entity.Property(sh => sh.CreatedAt).HasDefaultValueSql("NOW()");
            });

            // Configure Relationships
            // User -> Reservations
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // User -> ApprovedReservations
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Admin)
                .WithMany(u => u.ApprovedReservations)
                .HasForeignKey(r => r.AdminId)
                .OnDelete(DeleteBehavior.Restrict);

            // User -> StatusHistories
            modelBuilder.Entity<StatusHistory>()
                .HasOne(sh => sh.User)
                .WithMany(u => u.StatusHistories)
                .HasForeignKey(sh => sh.ChangedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // Ruangan -> Reservations
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Ruangan)
                .WithMany(r => r.Reservations)
                .HasForeignKey(r => r.RuanganId)
                .OnDelete(DeleteBehavior.Restrict);

            // Reservation -> StatusHistories
            modelBuilder.Entity<StatusHistory>()
                .HasOne(sh => sh.Reservation)
                .WithMany(r => r.StatusHistories)
                .HasForeignKey(sh => sh.ReservationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}