# ADR-002: Pessimistic Row-Level Locking (`FOR UPDATE`) untuk Pencegahan Double Booking

## Status
**Accepted**

## Tanggal
2026-10-29

## Konteks
Pada sistem reservasi ruangan, bentrok jadwal (*double booking*) adalah kegagalan kritis. 
Level isolasi default PostgreSQL adalah `READ COMMITTED`. Pada level ini, query pembacaan biasa (`SELECT ... AnyAsync`) tidak mengunci baris data. Dua transaksi simultan yang mengecek jadwal kosong pada saat bersamaan akan sama-sama menganggap ruangan tersedia sebelum salah satunya melakukan `COMMIT`, menyebabkan *double booking*.

## Keputusan
Menerapkan penguncian baris eksplisit (*Pessimistic Locking*) pada entitas induk `Spaces` menggunakan kueri PostgreSQL:
```csharp
var space = await _context.Spaces
    .FromSqlInterpolated($"SELECT * FROM \"Spaces\" WHERE \"Id\" = {request.SpaceId} FOR UPDATE")
    .FirstOrDefaultAsync();
```
Dan hal serupa untuk entitas `Vouchers` guna memastikan atomisitas kuota voucher.

## Alternatif yang Dipertimbangkan

### 1. Redis Distributed Lock (Redlock)
- **Kelebihan**: Mengantrekan request di memori Redis tanpa membebani connection pool database.
- **Kekurangan**: Menambahkan titik kegagalan kedua (ketergantungan pada availability Redis untuk integritas database). Rentan bocor jika kueri DB memakan waktu melebihi batas TTL kunci (lock expiry race condition). Abstraksi bawaan `IDistributedCache` di .NET tidak mendukung atomic conditional lock (`SET NX PX`).
- **Ditolak**: PostgreSQL adalah *Single Source of Truth* untuk data reservasi; penguncian database menjamin ACID secara mutlak dan otomatis lepas jika koneksi putus/crash.

### 2. Transaction Isolation Level Serializable
- **Kelebihan**: Murni fitur database bawaan tanpa sintaks raw SQL `FOR UPDATE`.
- **Kekurangan**: PostgreSQL tidak mengantrekan request; sebaliknya, PostgreSQL melempar error `40001: could not serialize access` dan membatalkan transaksi yang kalah cepat (*rollback storm*). Aplikasi wajib membuat retry loop manual yang kompleks.
- **Ditolak**: Pessimistic lock lebih efisien karena mengantrekan transaksi alih-alih melempar error pembatalan acak ke pengguna.

## Konsekuensi
- Membutuhkan dependensi `Microsoft.EntityFrameworkCore.Relational` di layer Application untuk mendukung method `FromSqlInterpolated`.
- Menahan 1 koneksi database selama durasi transaksi booking (biasanya $< 20\text{ ms}$).
- Menghilangkan kemungkinan double booking hingga 0% pada level database engine.
