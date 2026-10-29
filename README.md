# SpotBook - Space & Meeting Room Reservation API

RESTful Web API backend untuk sistem reservasi ruang pertemuan (*meeting room*, *co-working space*, *event hall*) berbasis **.NET 10 (`net10.0`)**, **PostgreSQL 16**, **Redis 7**, dan payment gateway **Midtrans Snap**, yang mengadopsi pola **Clean Architecture**.

---

## 🚀 Fitur Utama

- **Pola Clean Architecture**: Pemisahan tanggung jawab (*Separation of Concerns*) ke dalam 4 proyek terisolasi (`Domain`, `Application`, `Infrastructure`, `Api`).
- **Pencegahan Double Booking (ACID Safe)**: Penguncian baris database eksplisit (*Pessimistic Row-Level Locking* via `SELECT ... FOR UPDATE`) untuk memastikan tidak ada jadwal bentrok pada kondisi konkurensi tinggi.
- **Kupon Promosi & Diskon Terkendali**: Dukungan voucher persentase dan nominal tetap dengan kontrol kuota atomik serta pembatasan transaksi minimal Rp 1 untuk integrasi gateway.
- **Integrasi Midtrans Snap API**: Pembuatan tagihan pembayaran instan, validasi signature keamanan SHA-512 dua arah pada webhook, pengecekan *gross amount*, dan penanganan *retry* invoice yang kedaluwarsa.
- **High-Performance Caching (Redis)**: Menggunakan pola *Cache-Aside* dengan invalidasi otomatis saat terjadi mutasi data master ruangan dan kuota voucher.
- **Autentikasi JWT & Refresh Token Rotation**: Otentikasi stateless dengan token HMAC-SHA256 berumur 60 menit dan perpanjangan sesi via refresh token kriptografi.

---

## 🛠️ Tech Stack

* **Framework & Runtime**: .NET 10 SDK (`net10.0`), C# 13/14
* **Database & ORM**: PostgreSQL 16 via Entity Framework Core (`Npgsql.EntityFrameworkCore.PostgreSQL`)
* **Distributed Cache**: Redis 7 Alpine via `Microsoft.Extensions.Caching.StackExchangeRedis`
* **Payment Gateway**: Midtrans Snap API & Webhook Notification Engine
* **Interactive API Docs**: Swagger / OpenAPI & Scalar UI

---

## ⚡ Quick Start

```bash
# 1. Jalankan container database PostgreSQL & Redis
docker compose up -d

# 2. Terapkan migrasi database EF Core
dotnet ef database update --project src/SpotBook.Infrastructure --startup-project src/SpotBook.Api

# 3. Jalankan aplikasi API
dotnet run --project src/SpotBook.Api --launch-profile http
```

API akan aktif di:
* **Base URL**: `http://localhost:5135`
* **Scalar UI Docs**: `http://localhost:5135/scalar/v1`
* **Swagger UI Docs**: `http://localhost:5135/swagger`

---

## 📂 Struktur Repositori

```
SpotBook/
├── docker-compose.yaml                   # Layanan PostgreSQL 16 & Redis 7
├── SpotBook.slnx                         # Solution file
└── src/
    ├── SpotBook.Domain/                  # Core: Entities & Enums (zero dependency)
    ├── SpotBook.Application/             # Use cases: Services, DTOs, Interfaces, Exceptions
    ├── SpotBook.Infrastructure/          # Data access: EF Core (Npgsql), Redis Cache, JWT
    └── SpotBook.Api/                     # Entry point: Controllers, Middlewares, Program.cs
```

## 📌 API Endpoints Ringkas

* **Autentikasi**: `POST /api/auth/register`, `POST /api/auth/login`, `POST /api/auth/refresh-token`
* **Ruangan (Spaces)**: `GET /api/space`, `GET /api/space/{id}`, `POST /api/space` (Admin), `PUT /api/space/{id}` (Admin), `DELETE /api/space/{id}` (Admin)
* **Voucher Promosi**: `GET /api/vouchers` (Admin), `POST /api/vouchers` (Admin), `POST /api/vouchers/apply`
* **Pemesanan (Bookings)**: `POST /api/booking`, `GET /api/booking/my-bookings`, `GET /api/booking/{id}`, `POST /api/booking/{id}/cancel`
* **Pembayaran (Midtrans)**: `POST /api/payment/create-invoice`, `POST /api/payment/webhook`
