# SpotBook 🏢✨

> Platform backend modern untuk pemesanan ruang kerja, meeting room, dan event space secara instan tanpa ribet dan bebas bentrok jadwal.

---

## 🎯 Kenapa SpotBook?

Pernah mengalami susahnya cari ruang meeting yang pas, atau sudah booking tapi ternyata jadwalnya bertubrukan dengan orang lain? 

**SpotBook** hadir untuk menyelesaikan masalah itu. Backend API ini dirancang agar proses sewa ruangan berjalan mulus, cepat, dan transparan:
- **Jadwal Pasti Aman**: Sistem otomatis mengunci slot waktu secara real-time, jadi tidak akan ada cerita dua orang menyewa ruangan yang sama di jam yang sama (*no double-booking*).
- **Pembayaran Fleksibel**: Terintegrasi langsung dengan **Midtrans** (QRIS, Virtual Account, Transfer Bank, dan Kartu Kredit).
- **Promo & Kupon Diskon**: Pengguna bisa menghemat biaya sewa dengan memasukkan kode voucher promo.

---

## 👥 Apa Saja yang Bisa Dilakukan?

### 💼 Untuk Penyewa (Customer)
* **Cari Ruangan**: Lihat daftar ruangan yang tersedia lengkap dengan kapasitas, fasilitas, dan tarif per jam.
* **Booking Mudah**: Tentukan jam mulai dan selesai sewa sesuai kebutuhan (minimal 1 jam).
* **Gunakan Voucher**: Cek potongan harga secara real-time sebelum membayar.
* **Bayar Praktis**: Dapatkan invoice instan dan bayar lewat berbagai metode pembayaran.
* **Kelola Pesanan**: Pantau riwayat booking dan batalkan jika ada perubahan rencana.

### 🛠️ Untuk Pengelola (Admin)
* **Manajemen Ruangan**: Tambah ruangan baru, ubah harga sewa, fasilitas, atau tutup sementara ruangan yang sedang direnovasi.
* **Atur Promo**: Buat kupon diskon (persentase atau potongan langsung) lengkap dengan kuota pemakaian dan masa berlaku.

---

## 🚀 Menjalankan Project (Local Setup)

Cukup 3 langkah mudah untuk menjalankan SpotBook di komputermu:

```bash
# 1. Nyalakan database PostgreSQL & Redis (lewat Docker)
docker compose up -d

# 2. Update skema database
dotnet ef database update --project src/SpotBook.Infrastructure --startup-project src/SpotBook.Api

# 3. Jalankan server API
dotnet run --project src/SpotBook.Api --launch-profile http
```

Setelah jalan, kamu bisa langsung mencoba dan mengetes semua fiturnya lewat browser:
* **Interactive API Docs (Scalar)**: [http://localhost:5135/scalar/v1](http://localhost:5135/scalar/v1)
* **Swagger UI**: [http://localhost:5135/swagger](http://localhost:5135/swagger)

---

## 🧰 Teknologi yang Digunakan

* **Backend**: .NET 10 (C#) dengan pola Clean Architecture
* **Database**: PostgreSQL 16 (Entity Framework Core)
* **Cache**: Redis 7 (untuk loading ruangan yang super cepat)
* **Payment Gateway**: Midtrans Snap API & Webhook
