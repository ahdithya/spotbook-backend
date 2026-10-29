# Architecture Decision Records (ADR)

Koleksi catatan keputusan arsitektur teknis (*Architecture Decision Records*) yang diambil pada proyek **SpotBook**. Setiap keputusan mendokumentasikan konteks, alternatif yang dipertimbangkan, serta konsekuensi teknisnya.

## Daftar Keputusan

| ADR | Judul | Status | Tanggal |
| :---: | :--- | :---: | :---: |
| [ADR-001](./ADR-001-clean-architecture.md) | Pemilihan Pola Clean Architecture (4-Tier Project) | **Accepted** | 2026-10-14 |
| [ADR-002](./ADR-002-postgresql-pessimistic-locking-for-double-booking.md) | Pessimistic Row-Level Locking (`FOR UPDATE`) untuk Pencegahan Double Booking | **Accepted** | 2026-10-29 |
| [ADR-003](./ADR-003-redis-distributed-cache-aside.md) | Caching Data Aktif dengan Redis & Pola Cache-Aside | **Accepted** | 2026-10-27 |
| [ADR-004](./ADR-004-midtrans-payment-gateway-integration.md) | Integrasi Midtrans Snap & Validasi Webhook dengan Batas Minimum Rp 1 | **Accepted** | 2026-10-28 |
