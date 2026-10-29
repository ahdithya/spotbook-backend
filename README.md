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

## 📚 Dokumentasi Proyek Lengkap (`documents/`)

Seluruh dokumentasi teknis, arsitektur, dan panduan domain tersimpan di folder `documents/`:

1. [**Domain Context & Ubiquitous Language (`documents/CONTEXT.md`)**](./documents/CONTEXT.md)  
   Kamus istilah resmi domain, aturan invarian bisnis, dan diagram status pemesanan.
2. [**Arsitektur Sistem (`documents/ARCHITECTURE.md`)**](./documents/ARCHITECTURE.md)  
   Struktur 4-tier Clean Architecture, siklus request, strategi keamanan, dan pola desain.
3. [**Desain Basis Data (`documents/DATABASE-DESIGN.md`)**](./documents/DATABASE-DESIGN.md)  
   Diagram Relasi Entitas (ERD), spesifikasi kolom tabel, indeks performa, dan foreign keys.
4. [**Spesifikasi REST API (`documents/API-SPECIFICATION.md`)**](./documents/API-SPECIFICATION.md)  
   Kontrak endpoint lengkap (Auth, Spaces, Vouchers, Bookings, Payments), header, dan contoh respons JSON.
5. [**Architecture Decision Records (`documents/adr/`)**](./documents/adr/README.md)  
   Catatan keputusan teknis penting:
   - [ADR-001: Clean Architecture](./documents/adr/ADR-001-clean-architecture.md)
   - [ADR-002: PostgreSQL Row-Level Locking (`FOR UPDATE`)](./documents/adr/ADR-002-postgresql-pessimistic-locking-for-double-booking.md)
   - [ADR-003: Redis Distributed Caching](./documents/adr/ADR-003-redis-distributed-cache-aside.md)
   - [ADR-004: Integrasi Midtrans Snap & Webhook](./documents/adr/ADR-004-midtrans-payment-gateway-integration.md)
6. [**Panduan Pengembangan & Deployment (`documents/DEVELOPMENT-AND-DEPLOYMENT.md`)**](./documents/DEVELOPMENT-AND-DEPLOYMENT.md)  
   Prasyarat, konfigurasi environment variables, testing guide, dan panduan deployment produksi.
