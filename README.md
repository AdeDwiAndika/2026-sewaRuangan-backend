# Sewa Ruangan API (Backend)

Sistem backend untuk aplikasi peminjaman dan manajemen ruangan berbasis REST API.

## Teknologi yang Digunakan

- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- JWT Authentication
- Swagger UI

## Fitur Utama

- Autentikasi & otorisasi berbasis JWT
- CRUD peminjaman ruangan
- Approval/cancel/reject peminjaman
- Manajemen user & role (Admin, Staff, Dosen, Mahasiswa)
- Validasi & error handling
- Manajemen ruangan

## Panduan Instalasi

1. **Clone repository**
   ```bash
   git clone <repo-url>
   cd backend
   ```
2. **Atur koneksi database**
   - Edit `appsettings.json` dan `appsettings.Development.json` dengan string koneksi PostgreSQL Anda.
3. **Jalankan migrasi database**
   ```bash
   dotnet ef database update
   ```
4. **Jalankan aplikasi**
   ```bash
   dotnet run
   ```
5. **API dapat diakses di**
   - `https://localhost:5238/api/`

## Struktur Folder Penting

- `Controllers/` : Endpoint API
- `Models/` : Entity, DTO, Enum
- `Data/` : DbContext, migrasi
- `Services/` : Bisnis logic

## Catatan

- Pastikan PostgreSQL sudah berjalan.
- Default port: 5238 (ubah di `launchSettings.json` jika perlu).
- Gunakan tool seperti Postman / Swagger untuk testing endpoint.
