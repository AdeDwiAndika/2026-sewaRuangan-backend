using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SewaRuangan.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreatePostgreSQL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ruangans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    kode_ruangan = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    nama_ruangan = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Gedung = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Lantai = table.Column<int>(type: "integer", nullable: false),
                    Kapasitas = table.Column<int>(type: "integer", nullable: false),
                    Fasilitas = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "tersedia"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ruangans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    phone_number = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    kode_peminjaman = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    ruangan_id = table.Column<int>(type: "integer", nullable: false),
                    Keperluan = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    jumlah_peserta = table.Column<int>(type: "integer", nullable: false),
                    tanggal_peminjaman = table.Column<DateTime>(type: "date", nullable: false),
                    waktu_mulai = table.Column<TimeSpan>(type: "time", nullable: false),
                    waktu_selesai = table.Column<TimeSpan>(type: "time", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "menunggu"),
                    admin_id = table.Column<int>(type: "integer", nullable: true),
                    disetujui_pada = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    catatan_admin = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.Id);
                    table.CheckConstraint("CK_Reservation_Time", "\"waktu_selesai\" > \"waktu_mulai\"");
                    table.ForeignKey(
                        name: "FK_Reservations_Ruangans_ruangan_id",
                        column: x => x.ruangan_id,
                        principalTable: "Ruangans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservations_Users_admin_id",
                        column: x => x.admin_id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservations_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StatusHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    reservation_id = table.Column<int>(type: "integer", nullable: false),
                    status_sebelum = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status_sesudah = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    changed_by = table.Column<int>(type: "integer", nullable: false),
                    catatan = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StatusHistories_Reservations_reservation_id",
                        column: x => x.reservation_id,
                        principalTable: "Reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StatusHistories_Users_changed_by",
                        column: x => x.changed_by,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_admin_id",
                table: "Reservations",
                column: "admin_id");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_kode_peminjaman",
                table: "Reservations",
                column: "kode_peminjaman",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ruangan_id",
                table: "Reservations",
                column: "ruangan_id");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_user_id",
                table: "Reservations",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Ruangans_kode_ruangan",
                table: "Ruangans",
                column: "kode_ruangan",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StatusHistories_changed_by",
                table: "StatusHistories",
                column: "changed_by");

            migrationBuilder.CreateIndex(
                name: "IX_StatusHistories_reservation_id",
                table: "StatusHistories",
                column: "reservation_id");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StatusHistories");

            migrationBuilder.DropTable(
                name: "Reservations");

            migrationBuilder.DropTable(
                name: "Ruangans");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
