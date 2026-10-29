# ADR-004: Integrasi Midtrans Snap & Validasi Webhook dengan Batas Minimum Rp 1

## Status
**Accepted**

## Tanggal
2026-10-28

## Konteks
Sistem reservasi SpotBook memerlukan integrasi gateway pembayaran untuk memproses berbagai metode pembayaran lokal di Indonesia (QRIS, Virtual Account, Kartu Kredit, E-Wallet). Tantangan integrasi:
1. Validasi keamanan webhook dari pihak luar.
2. Kasus penggunaan voucher 100% diskon yang menghasilkan total tagihan Rp 0 (Midtrans menolak transaksi bernilai nol).
3. Penanganan Snap Token yang kedaluwarsa tanpa merusak siklus hidup pesanan.

## Keputusan
1. **Menggunakan Midtrans Snap API**: Menghasilkan `snapToken` dan `redirect_url` untuk antarmuka pembayaran pembayaran terpusat.
2. **Batas Minimal Transaksi Rp 1**: Pada kalkulasi voucher diskon 100%, total tagihan otomatis dipatok minimal **Rp 1** agar valid diproses oleh Midtrans API dan perbankan.
3. **Verifikasi Hash Webhook SHA-512**: Menghitung kembali hash keamanan:
   $$\text{SHA512}(\text{OrderId} + \text{StatusCode} + \text{GrossAmount} + \text{ServerKey})$$
   dan mencocokkan nilai `GrossAmount` dengan `booking.TotalAmount` di database.
4. **Idempotency & Non-Reactivation**:
   - Jika status pemesanan sudah `Paid`, notifikasi berulang diabaikan.
   - Jika pemesanan sudah `Cancelled`, webhook settlement tidak boleh mengubah status booking kembali menjadi `Paid`.
5. **Invoice Expiration Retry**: Jika token Snap sudah expired, sistem mengizinkan generate Snap token baru dengan memperbarui kode booking unik agar diterima oleh sistem Midtrans.

## Konsekuensi
- Menghilangkan celah manipulasi harga via webhook.
- Pengguna yang mendapatkan voucher gratis tetap dapat menyelesaikan alur checkout resmi tanpa error gateway.
- Pesanan yang expired dapat dibayar ulang tanpa memaksa pengguna mengulang proses reservasi dari awal.
