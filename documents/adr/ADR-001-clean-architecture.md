# ADR-001: Pemilihan Pola Clean Architecture (4-Tier Project)

## Status
**Accepted**

## Tanggal
2026-10-14

## Konteks
Sistem reservasi SpotBook memerlukan arsitektur modular yang memisahkan logika bisnis dari teknologi infrastruktur (EF Core, PostgreSQL, Redis, HTTP Web API). Kebutuhan utama:
- Logika bisnis dapat diuji secara terisolasi tanpa database riil.
- Penggantian atau pembaruan library pihak ketiga tidak boleh merusak aturan bisnis.
- Kemudahan navigasi kode bagi tim pengembang.

## Keputusan
Membagi solusi (.NET Solution) ke dalam 4 proyek terpisah:
1. `SpotBook.Domain`: Entitas dan Enum murni tanpa dependensi.
2. `SpotBook.Application`: Use cases, antarmuka service, DTOs, dan exceptions.
3. `SpotBook.Infrastructure`: Implementasi data access via EF Core (Npgsql), Redis caching, dan token JWT.
4. `SpotBook.Api`: Host ASP.NET Core, Controllers, dan Middlewares.

## Alternatif yang Dipertimbangkan

### 1. Monolithic Single Project (Traditional MVC / CRUD)
- **Kelebihan**: Cepat dibuat di awal.
- **Kekurangan**: Controller langsung menyentuh DbContext, logika bisnis bercampur dengan HTTP concerns, sulit diuji unit test.
- **Ditolak**: Menimbulkan technical debt tinggi dalam jangka panjang.

### 2. Vertical Slice Architecture (Feature Folders)
- **Kelebihan**: Setiap fitur mandiri dari controller hingga query database.
- **Kekurangan**: Terlalu banyak duplikasi model untuk proyek reservasi berskala menengah yang berbagi banyak entitas relasional.
- **Ditolak**: Clean Architecture lebih konsisten dengan pola kerja tim .NET saat ini.

## Konsekuensi
- Mengharuskan pemetaan DTO ke Domain Entity.
- Dependency Injection wajib dikonfigurasi secara eksplisit di layer `Api/Program.cs`.
- Struktur folder sangat teratur dan mudah di-*maintain*.
