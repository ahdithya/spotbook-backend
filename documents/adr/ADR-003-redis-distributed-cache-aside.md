# ADR-003: Caching Data Aktif dengan Redis & Pola Cache-Aside

## Status
**Accepted**

## Tanggal
2026-10-27

## Konteks
Kueri pembacaan daftar ruangan (`GET /api/space`) dan validasi voucher (`POST /api/vouchers/apply`) merupakan titik akses dengan frekuensi baca tinggi (*read-heavy*). Menembak database PostgreSQL secara terus-menerus untuk data master yang jarang berubah menurunkan *throughput* API secara keseluruhan.

## Keputusan
1. Menggunakan **Redis 7** sebagai sistem cache terdistribusi melalui abstraksi `IDistributedCache` (`Microsoft.Extensions.Caching.StackExchangeRedis`).
2. Menerapkan pola **Cache-Aside**:
   - Pembacaan memeriksa Redis terlebih dahulu dengan TTL 15–30 menit.
   - Mutasi data (Create/Update/Delete ruangan atau perubahan status voucher) langsung menghapus key terkait di Redis via `RemoveAsync()`.

## Alternatif yang Dipertimbangkan

### 1. In-Memory Cache (.NET IMemoryCache)
- **Kelebihan**: Sangat cepat, nol dependensi jaringan luar.
- **Kekurangan**: Terikat pada satu instance server. Jika aplikasi di-scale ke multi-instance/container, cache antar server tidak sinkron (*cache drift*).
- **Ditolak**: SpotBook dirancang untuk arsitektur terdistribusi horizontal.

### 2. Tanpa Caching (Direct PostgreSQL Query)
- **Kelebihan**: Data selalu 100% konsisten.
- **Kekurangan**: Latensi pembacaan tinggi ($10\text{--}30\text{ ms}$ vs $<1\text{ ms}$ di Redis) dan membebani IOPS database.
- **Ditolak**: Penambahan Redis memberikan peningkatan respon hingga $10\times$ lipat.

## Konsekuensi
- Memerlukan container Redis yang berjalan bersama aplikasi (`spotbook-redis`).
- Wajib memastikan setiap mutasi (termasuk pada webhook pembayaran) memicu invalidasi cache yang relevan.
