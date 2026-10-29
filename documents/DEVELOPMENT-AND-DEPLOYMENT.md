# Panduan Pengembangan & Deployment: SpotBook

Dokumen ini memuat panduan lengkap penyiapan lingkungan pengembangan lokal (*local development*), migrasi basis data, pengujian, dan pedoman deployment produksi untuk **SpotBook**.

---

## 1. Prasyarat Sistem

* **.NET 10 SDK** (`net10.0`) atau versi lebih baru.
* **Docker & Docker Compose** (untuk menjalankan PostgreSQL 16 & Redis 7).
* **Git**.
* *Editor/IDE disarankan*: Visual Studio 2024 / VS Code dengan C# Dev Kit.

---

## 2. Pengaturan Lingkungan Lokal (Quick Start)

### Langkah 1: Jalankan Layanan Infrastruktur (PostgreSQL & Redis)
Di root direktori proyek, jalankan container menggunakan Docker Compose:
```bash
docker compose up -d
```
Pastikan kedua container telah aktif:
```bash
docker compose ps
```
* **PostgreSQL**: Port `5432` (User: `spotbook_user`, Password: `spotbook_password`, DB: `SpotBookDb`)
* **Redis**: Port `6379`

### Langkah 2: Terapkan Migrasi Database (EF Core)
Jalankan migrasi Entity Framework Core ke database PostgreSQL:
```bash
dotnet ef database update --project src/SpotBook.Infrastructure --startup-project src/SpotBook.Api
```

### Langkah 3: Jalankan Backend API
Jalankan aplikasi API menggunakan profil HTTP:
```bash
dotnet run --project src/SpotBook.Api --launch-profile http
```
Aplikasi akan aktif dan mendengarkan permintaan pada:
* **API Base URL**: `http://localhost:5135`
* **Dokumentasi Scalar UI**: `http://localhost:5135/scalar/v1`
* **Dokumentasi Swagger UI**: `http://localhost:5135/swagger`

---

## 3. Konfigurasi Lingkungan (`appsettings.json`)

Struktur konfigurasi utama yang diperlukan pada file `appsettings.json` (atau *Environment Variables* di server produksi):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=SpotBookDb;Username=spotbook_user;Password=spotbook_password",
    "Redis": "localhost:6379"
  },
  "JwtSettings": {
    "Secret": "KunciRahasiaSpotBookSuperAmanMinimal32KarakterHarusPanjang12345!",
    "Issuer": "SpotBookApi",
    "Audience": "SpotBookClient",
    "ExpiryMinutes": 60
  },
  "PaymentSettings": {
    "ServerKey": "YOUR_MIDTRANS_SERVER_KEY",
    "ClientKey": "YOUR_MIDTRANS_CLIENT_KEY",
    "SnapUrl": "https://app.sandbox.midtrans.com/snap/v1/transactions"
  }
}
```

> **Catatan Keamanan Produksi**:
> Jangan menyimpan `JwtSettings:Secret` atau `PaymentSettings:ServerKey` riil di dalam kontrol versi Git. Gunakan *Secret Manager* (.NET User Secrets pada pengembangan lokal) atau *Environment Variables* / *Azure Key Vault* / *AWS Secrets Manager* di produksi.

---

## 4. Panduan Pengujian (Testing)

### Build Verification
Pastikan seluruh project terkompilasi tanpa error maupun warning:
```bash
dotnet build
```

### Skenario Pengujian Alur Transaksi Lengkap (End-to-End)
Alur pengujian reservasi mencakup:
1. Registrasi & Login pengguna (`/api/auth/register` & `/api/auth/login`).
2. Admin membuat ruangan (`/api/space`) dan voucher promosi (`/api/vouchers`).
3. Pengguna mengajukan pemesanan (`/api/booking`) dengan jadwal tertentu.
4. Validasi bentrok jadwal (mencegah *double booking* jika dipesan pada jam yang sama).
5. Pembuatan tagihan Midtrans Snap (`/api/payment/create-invoice`).
6. Simulasi webhook settlement Midtrans (`/api/payment/webhook`) dengan verifikasi signature hash SHA-512.
7. Pemeriksaan perubahan status pemesanan menjadi `Paid`.

---

## 5. Pertimbangan Deployment Produksi

1. **Reverse Proxy & SSL/TLS**:
   Tempatkan API di belakang reverse proxy seperti **Nginx**, **Caddy**, atau **Cloudflare** dengan sertifikat SSL/TLS HTTPS aktif.
2. **Koneksi Database & Pool Sizing**:
   Atur `MaxPoolSize` pada string koneksi PostgreSQL Npgsql (default: 100) sesuai kapasitas RAM server basis data.
3. **Pembersihan Logika Expired Booking**:
   Disarankan menjalankan background job / cron worker periodik untuk membatalkan pesanan yang belum dibayar melebihi 15–30 menit agar slot ruangan dapat segera dilepas bagi pelanggan lain.
