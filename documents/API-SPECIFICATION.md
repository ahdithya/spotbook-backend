# Spesifikasi Kontrak REST API: SpotBook

Dokumen ini memuat kontrak antarmuka pemrograman aplikasi (REST API) lengkap untuk **SpotBook**, mencakup format pertukaran data, autentikasi, daftar endpoint, skema payload, dan kode status HTTP.

---

## 1. Standar Pertukaran Data & Respons

Seluruh endpoint API mengembalikan data berformat JSON dengan membungkusnya dalam format seragam **`ApiResponse<T>`**:

### Respons Sukses
```json
{
  "success": true,
  "message": "Pemesanan berhasil dibuat.",
  "data": { ... },
  "errors": null
}
```

### Respons Gagal / Error
```json
{
  "success": false,
  "message": "Jadwal pada ruangan ini sudah dipesan orang lain.",
  "data": null,
  "errors": ["Detail kesalahan jika ada"]
}
```

---

## 2. Autentikasi & Header

Sebagian besar endpoint memerlukan token **JWT Bearer** yang dikirimkan pada header HTTP:
```http
Authorization: Bearer <AccessToken>
Content-Type: application/json
```

---

## 3. Daftar Endpoint

### A. Autentikasi (`/api/auth`)

#### 1. Registrasi Akun Baru
* **Method & Path**: `POST /api/auth/register`
* **Akses**: Publik
* **Request Body**:
  ```json
  {
    "fullName": "Budi Santoso",
    "email": "budi@example.com",
    "phoneNumber": "081234567890",
    "password": "Password123!"
  }
  ```
* **Status**: `200 OK`
* **Response Data**:
  ```json
  {
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "fullName": "Budi Santoso",
    "email": "budi@example.com",
    "role": "Customer",
    "accessToken": "eyJhbGciOiJIUzI1Ni...",
    "refreshToken": "xYz123Random64Bytes..."
  }
  ```

#### 2. Login
* **Method & Path**: `POST /api/auth/login`
* **Akses**: Publik
* **Request Body**:
  ```json
  {
    "email": "budi@example.com",
    "password": "Password123!"
  }
  ```
* **Status**: `200 OK` (mengembalikan AccessToken dan RefreshToken baru).

#### 3. Refresh Token
* **Method & Path**: `POST /api/auth/refresh-token`
* **Akses**: Publik
* **Request Body**:
  ```json
  {
    "accessToken": "eyJhbGciOiJIUzI1Ni...",
    "refreshToken": "xYz123Random64Bytes..."
  }
  ```
* **Status**: `200 OK`.

---

### B. Ruangan / Spaces (`/api/space`)

#### 1. Dapatkan Seluruh Ruangan Aktif
* **Method & Path**: `GET /api/space`
* **Akses**: Publik (Menggunakan Cache Redis `spaces:active`)
* **Status**: `200 OK`
* **Response Data**:
  ```json
  [
    {
      "id": "c1f2e3d4-...",
      "name": "Meeting Room Alpha",
      "description": "Ruangan berkapasitas 10 orang dengan proyektor",
      "capacity": 10,
      "pricePerHour": 150000.0,
      "isActive": true,
      "createdAt": "2026-10-29T08:00:00Z"
    }
  ]
  ```

#### 2. Detail Ruangan
* **Method & Path**: `GET /api/space/{id}`
* **Akses**: Publik
* **Status**: `200 OK` / `404 Not Found`

#### 3. Tambah Ruangan Baru
* **Method & Path**: `POST /api/space`
* **Akses**: `[Authorize(Roles = "Admin")]`
* **Request Body**:
  ```json
  {
    "name": "Creative Studio",
    "description": "Studio kedap suara",
    "capacity": 6,
    "pricePerHour": 200000.0
  }
  ```
* **Status**: `201 Created`

#### 4. Perbarui Ruangan
* **Method & Path**: `PUT /api/space/{id}`
* **Akses**: `[Authorize(Roles = "Admin")]`
* **Status**: `200 OK`

#### 5. Nonaktifkan Ruangan (Soft Delete)
* **Method & Path**: `DELETE /api/space/{id}`
* **Akses**: `[Authorize(Roles = "Admin")]`
* **Status**: `200 OK`

---

### C. Voucher Diskon (`/api/vouchers`)

#### 1. Daftar Voucher
* **Method & Path**: `GET /api/vouchers`
* **Akses**: `[Authorize(Roles = "Admin")]`
* **Status**: `200 OK`

#### 2. Buat Voucher
* **Method & Path**: `POST /api/vouchers`
* **Akses**: `[Authorize(Roles = "Admin")]`
* **Request Body**:
  ```json
  {
    "code": "HEMAT50",
    "discountType": 1,
    "discountValue": 50,
    "maxDiscountAmount": 100000,
    "minOrderAmount": 150000,
    "quota": 100,
    "validFrom": "2026-11-01T00:00:00Z",
    "validUntil": "2026-11-30T23:59:59Z"
  }
  ```
* **Status**: `200 OK`

#### 3. Cek & Terapkan Voucher (Preview)
* **Method & Path**: `POST /api/vouchers/apply`
* **Akses**: `[Authorize]`
* **Request Body**:
  ```json
  {
    "code": "HEMAT50",
    "orderAmount": 300000.0
  }
  ```
* **Status**: `200 OK`
* **Response Data**:
  ```json
  {
    "voucherId": "...",
    "code": "HEMAT50",
    "originalAmount": 300000.0,
    "discountAmount": 100000.0,
    "finalAmount": 200000.0,
    "message": "Voucher berhasil diterapkan!"
  }
  ```

---

### D. Pemesanan / Booking (`/api/booking`)

#### 1. Buat Reservasi Ruangan
* **Method & Path**: `POST /api/booking`
* **Akses**: `[Authorize]`
* **Catatan**: Menggunakan transaksi atomik dan row lock `SELECT ... FOR UPDATE` pada PostgreSQL.
* **Request Body**:
  ```json
  {
    "spaceId": "c1f2e3d4-...",
    "startTime": "2026-11-02T09:00:00Z",
    "endTime": "2026-11-02T12:00:00Z",
    "voucherCode": "HEMAT50"
  }
  ```
* **Status**: `201 Created`
* **Response Data**:
  ```json
  {
    "id": "b8a9c0d1-...",
    "bookingCode": "SB-20261102-4A1B",
    "spaceId": "c1f2e3d4-...",
    "spaceName": "Meeting Room Alpha",
    "startTime": "2026-11-02T09:00:00Z",
    "endTime": "2026-11-02T12:00:00Z",
    "durationInHours": 3.0,
    "basePrice": 450000.0,
    "discountAmount": 100000.0,
    "totalAmount": 350000.0,
    "status": "PendingPayment",
    "createdAt": "2026-10-29T10:00:00Z"
  }
  ```

#### 2. Riwayat Pemesanan Saya
* **Method & Path**: `GET /api/booking/my-bookings`
* **Akses**: `[Authorize]`
* **Status**: `200 OK`

#### 3. Detail Pemesanan
* **Method & Path**: `GET /api/booking/{id}`
* **Akses**: `[Authorize]`
* **Status**: `200 OK`

#### 4. Batalkan Pemesanan
* **Method & Path**: `POST /api/booking/{id}/cancel`
* **Akses**: `[Authorize]`
* **Status**: `200 OK` (Hanya berlaku untuk status `PendingPayment`).

---

### E. Pembayaran & Gateway (`/api/payment`)

#### 1. Buat Tagihan Pembayaran (Midtrans Snap)
* **Method & Path**: `POST /api/payment/create-invoice`
* **Akses**: `[Authorize]`
* **Request Body**:
  ```json
  {
    "bookingId": "b8a9c0d1-...",
    "paymentMethod": "MidtransSnap"
  }
  ```
* **Status**: `200 OK`
* **Response Data**:
  ```json
  {
    "paymentTransactionId": "...",
    "bookingId": "b8a9c0d1-...",
    "bookingCode": "SB-20261102-4A1B",
    "amount": 350000.0,
    "paymentMethod": "MidtransSnap",
    "paymentGatewayRef": "snap-token-xyz-123",
    "paymentUrl": "https://app.sandbox.midtrans.com/snap/v2/vtweb/...",
    "status": "Unpaid",
    "createdAt": "2026-10-29T10:05:00Z"
  }
  ```

#### 2. Webhook Midtrans
* **Method & Path**: `POST /api/payment/webhook`
* **Akses**: Publik (`[AllowAnonymous]`)
* **Request Body**:
  ```json
  {
    "order_id": "SB-20261102-4A1B",
    "status_code": "200",
    "gross_amount": "350000.00",
    "transaction_status": "settlement",
    "signature_key": "a1b2c3d4e5...sha512",
    "fraud_status": "accept"
  }
  ```
* **Status**: `200 OK`
