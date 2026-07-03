# PHASE_PROMPTS.md — CareOps

Gunakan prompt di bawah ini langsung ke IBM Bob per phase.
**Selalu buka AGENTS.md di awal setiap sesi IBM Bob.**

---

## Phase 1 — Infrastructure Setup + Patient Service

**Tujuan**: Setup docker-compose infrastructure, scaffold patient-service lengkap sebagai template untuk service lainnya.

**Prompt untuk IBM Bob:**
```
Baca AGENTS.md dan semua file di .bob/rules/ sebelum mulai.

Saya membangun CareOps dengan microservice architecture.
Stack: .NET 10, EF Core, MassTransit + RabbitMQ, Keycloak, YARP, Next.js 14.
TIDAK pakai Event Sourcing. TIDAK pakai Marten.

Tolong lakukan Phase 1:

1. Buat docker-compose.infra.yml untuk infrastructure lokal:
   - PostgreSQL (satu instance, multiple databases: patient_db, appointment_db, pharmacy_db, billing_db, keycloak_db)
   - RabbitMQ dengan management UI (port 15672)
   - Keycloak (port 8080) dengan realm config dasar

2. Scaffold patient-service sebagai template service:
   Struktur: PatientService.Api / Application / Domain / Infrastructure
   
   Domain:
   - Entity: Patient (Id, FirstName, LastName, DateOfBirth, Gender, PhoneNumber, Email, Address, MedicalRecordNumber, CreatedAt)
   - ValueObject: MedicalRecordNumber (auto-generated format: MRN-YYYYMMDD-XXXX)
   - Exception: PatientNotFoundException, DuplicateMedicalRecordException
   
   Application:
   - RegisterPatientCommand + Handler → save ke DB → publish PatientRegistered event
   - UpdatePatientCommand + Handler
   - GetPatientByIdQuery + Handler
   - GetPatientsQuery + Handler (dengan pagination: page, pageSize, search)
   - ValidationBehavior + LoggingBehavior pipeline
   
   Infrastructure:
   - PatientDbContext dengan EF Core + Npgsql
   - Seed data: 5 sample patients
   - MassTransit setup: publish PatientRegistered event ke RabbitMQ
   
   Api:
   - POST /patients → RegisterPatientCommand
   - PUT /patients/{id} → UpdatePatientCommand
   - GET /patients/{id} → GetPatientByIdQuery
   - GET /patients → GetPatientsQuery
   - GET /health → health check (DB + RabbitMQ)

3. JWT Auth di patient-service:
   - Validasi token dari Keycloak
   - Role-based: GET /patients boleh receptionist, doctor, pharmacist, cashier, admin
   - POST /patients hanya receptionist + admin
   - GET /patients/{id} → patient hanya bisa akses data sendiri

Ikuti semua aturan di .bob/rules/ dan .bob/rules-code/01-dotnet.md.
```

**Definition of Done:**
- [ ] `docker-compose -f docker-compose.infra.yml up` jalan tanpa error
- [ ] patient-service build dan run tanpa error
- [ ] POST /patients berhasil simpan ke PostgreSQL
- [ ] GET /patients return list dengan pagination
- [ ] `PatientRegistered` event muncul di RabbitMQ management UI
- [ ] GET /health return healthy
- [ ] JWT validation bekerja (request tanpa token → 401)

---

## Phase 2 — Appointment Service + API Gateway

**Tujuan**: Scaffold appointment-service dan setup YARP API Gateway.

**Prompt untuk IBM Bob:**
```
Baca AGENTS.md dan .bob/rules-code/01-dotnet.md sebelum mulai.

Phase 1 selesai. patient-service sudah running.
Lanjut Phase 2 — appointment-service + API Gateway.

1. Scaffold appointment-service (struktur sama seperti patient-service):
   
   Domain:
   - Entity: Doctor (Id, Name, Specialization, LicenseNumber, Schedule)
   - Entity: Appointment (Id, PatientId, DoctorId, ScheduledAt, Status, Notes, CreatedAt)
   - Enum: AppointmentStatus (Scheduled, InProgress, Completed, Cancelled)
   - Exception: AppointmentNotFoundException, DoctorNotAvailableException, AppointmentConflictException
   
   Application:
   - CreateAppointmentCommand → cek ketersediaan dokter → save → publish AppointmentCreated
   - CancelAppointmentCommand → update status → publish AppointmentCancelled
   - CompleteAppointmentCommand → update status Completed → publish AppointmentCompleted
   - CreatePrescriptionCommand → buat resep → publish PrescriptionCreated
   - GetAppointmentByIdQuery
   - GetAppointmentsQuery (filter: doctorId, patientId, date, status)
   - GetDoctorScheduleQuery
   
   Domain Events yang di-publish:
   - AppointmentCreated (appointmentId, patientId, doctorId, scheduledAt)
   - AppointmentCancelled (appointmentId, patientId, reason)
   - AppointmentCompleted (appointmentId, patientId, doctorId)
   - PrescriptionCreated (prescriptionId, appointmentId, patientId, items[])
   
   Role access:
   - POST /appointments → receptionist, admin
   - GET /appointments → semua role (filter by own untuk patient)
   - POST /appointments/{id}/complete + POST /appointments/{id}/prescriptions → doctor, admin
   - POST /appointments/{id}/cancel → receptionist, doctor, admin

2. Setup API Gateway (YARP):
   - Project: HospitalGateway
   - Route /api/patients/** → patient-service:5001
   - Route /api/appointments/** → appointment-service:5002
   - Middleware: validate JWT dari Keycloak
   - Forward header X-User-Id dan X-User-Roles ke downstream service

3. Update docker-compose.yml (full stack):
   - Tambahkan patient-service, appointment-service, api-gateway
   - Network: semua service dalam satu Docker network
   - Depends_on dengan health check

Ikuti .bob/rules/01-architecture.md — komunikasi antar service HANYA via events.
```

**Definition of Done:**
- [ ] appointment-service running di port 5002
- [ ] API Gateway running di port 5000
- [ ] Request via Gateway ke `/api/patients` berhasil di-forward ke patient-service
- [ ] `AppointmentCreated` event muncul di RabbitMQ
- [ ] `PrescriptionCreated` event muncul di RabbitMQ
- [ ] Role check bekerja (doctor mencoba POST /appointments → 403)

---

## Phase 3 — Pharmacy Service + Notification Service

**Tujuan**: Pharmacy service dengan stock management + notification service sebagai event consumer.

**Prompt untuk IBM Bob:**
```
Baca AGENTS.md dan .bob/rules-code/01-dotnet.md sebelum mulai.

Phase 2 selesai. Lanjut Phase 3 — pharmacy-service + notification-service.

1. Scaffold pharmacy-service:
   
   Domain:
   - Entity: Medicine (Id, Name, GenericName, Category, Unit, StockQuantity, MinimumStock, Price, ExpiryDate)
   - Entity: Prescription (Id, PrescriptionId, PatientId, AppointmentId, Status, Items[], CreatedAt)
   - Entity: PrescriptionItem (MedicineId, MedicineName, Quantity, Dosage, Instructions)
   - Entity: StockMovement (Id, MedicineId, Type, Quantity, Reason, CreatedAt)
   - Enum: PrescriptionStatus (Pending, Dispensed, Cancelled)
   - Enum: StockMovementType (In, Out, Adjustment)
   - Exception: InsufficientStockException, MedicineNotFoundException, PrescriptionAlreadyDispensedException
   
   Application:
   - DispensePrescriptionCommand → kurangi stok → publish PrescriptionDispensed
   - AddStockCommand → tambah stok → catat StockMovement
   - GetInventoryQuery (dengan filter: low stock, expired soon)
   - GetPrescriptionByIdQuery
   
   Consumer (dari RabbitMQ):
   - PrescriptionCreatedConsumer → terima PrescriptionCreated event dari appointment-service → simpan prescription record dengan status Pending
   
   Role access:
   - POST /pharmacy/dispense → pharmacist, admin
   - GET /pharmacy/inventory → pharmacist, admin
   - POST /pharmacy/stock → pharmacist, admin

2. Scaffold notification-service:
   - TIDAK punya database — pure event consumer
   - Consumers:
     - PatientRegisteredConsumer → log "Welcome" notification
     - AppointmentCreatedConsumer → log reminder notification
     - AppointmentCancelledConsumer → log cancellation notification
     - PrescriptionDispensedConsumer → log "obat siap diambil" notification
     - BillGeneratedConsumer → log "tagihan tersedia" notification
     - BillPaidConsumer → log "pembayaran berhasil" notification
   - Untuk sekarang: log ke console + simpan ke in-memory list (nanti bisa extend ke email/push)
   - Expose GET /notifications/{userId} → return list notifikasi user (in-memory)

3. Update docker-compose.yml:
   - Tambahkan pharmacy-service (port 5003)
   - Tambahkan notification-service (port 5005)
   - Update API Gateway routes:
     - /api/pharmacy/** → pharmacy-service:5003

Pastikan PrescriptionCreatedConsumer di pharmacy-service idempotent.
```

**Definition of Done:**
- [ ] pharmacy-service running di port 5003
- [ ] notification-service running di port 5005
- [ ] Buat appointment dengan prescription → PrescriptionCreated event → pharmacy-service terima dan simpan
- [ ] Dispense prescription → stok berkurang → PrescriptionDispensed event → notification-service log
- [ ] GET /pharmacy/inventory return daftar obat dengan stok
- [ ] Low stock alert muncul kalau stok < minimumStock

---

## Phase 4 — Billing Service

**Tujuan**: Billing service lengkap dengan kalkulasi tagihan dan pembayaran.

**Prompt untuk IBM Bob:**
```
Baca AGENTS.md dan .bob/rules-code/01-dotnet.md sebelum mulai.

Phase 3 selesai. Lanjut Phase 4 — billing-service.

Scaffold billing-service:

Domain:
- Entity: Bill (Id, PatientId, AppointmentId, Status, LineItems[], TotalAmount, PaidAt, CreatedAt)
- Entity: BillLineItem (Description, Quantity, UnitPrice, Amount)
- Enum: BillStatus (Draft, Issued, Paid, Cancelled)
- Exception: BillNotFoundException, BillAlreadyPaidException

Application:
- Consumers (dari RabbitMQ):
  - AppointmentCreatedConsumer → buat Bill dengan status Draft, line item: biaya konsultasi
  - PrescriptionDispensedConsumer → tambah line items obat ke Bill yang ada
  - AppointmentCancelledConsumer → cancel Bill terkait

- Commands:
  - IssueBillCommand → ubah status Draft → Issued → publish BillGenerated
  - ProcessPaymentCommand → ubah status Issued → Paid → publish BillPaid
  - CancelBillCommand → ubah status → Cancelled

- Queries:
  - GetBillByIdQuery
  - GetBillsByPatientQuery
  - GetBillsSummaryQuery (untuk cashier dashboard: total hari ini, pending, paid)

Role access:
- GET /billing → cashier, admin, patient (own only)
- POST /billing/{id}/issue → cashier, admin
- POST /billing/{id}/pay → cashier, admin
- POST /billing/{id}/cancel → cashier, admin

Update API Gateway:
- /api/billing/** → billing-service:5004

Update docker-compose.yml:
- Tambahkan billing-service (port 5004)

WAJIB pakai decimal untuk semua kalkulasi amount.
Consumer harus idempotent — cek apakah bill untuk appointmentId sudah ada sebelum buat baru.
```

**Definition of Done:**
- [ ] billing-service running di port 5004
- [ ] Flow lengkap: buat appointment → Bill Draft terbuat otomatis
- [ ] Dispense obat → line item obat masuk ke Bill
- [ ] Issue + Pay bill → status berubah → BillPaid event diterima notification-service
- [ ] GET /billing/{id} return detail tagihan dengan semua line items
- [ ] Patient hanya bisa lihat bill miliknya sendiri

---

## Phase 5 — Next.js Frontend + Keycloak Setup

**Tujuan**: Setup Keycloak realm, Next.js frontend dengan auth dan semua halaman utama.

**Prompt untuk IBM Bob:**
```
Baca AGENTS.md dan .bob/rules-code/02-nextjs.md sebelum mulai.

Phase 4 selesai. Semua backend service sudah running.
Lanjut Phase 5 — Keycloak setup + Next.js frontend.

1. Keycloak realm setup (keycloak/realm-export.json):
   - Realm: careops
   - Client: careops-frontend (public, PKCE, redirect: http://localhost:3000/*)
   - Client: api-gateway (confidential)
   - Roles: admin, receptionist, doctor, pharmacist, cashier, patient
   - Sample users:
     - admin@hospital.com / Admin123! → role: admin
     - reception@hospital.com / Admin123! → role: receptionist
     - doctor@hospital.com / Admin123! → role: doctor
     - pharmacist@hospital.com / Admin123! → role: pharmacist
     - cashier@hospital.com / Admin123! → role: cashier
     - patient@hospital.com / Admin123! → role: patient

2. Next.js 14 setup:
   - next-auth v5 dengan Keycloak provider
   - Middleware: protect semua route kecuali /login
   - Helper: hasRole(session, role) → boolean
   - Typed API client di lib/api.ts

3. Halaman:
   
   /login → redirect ke Keycloak, tampilkan loading
   
   / (dashboard) → summary cards sesuai role:
   - Admin: total pasien, appointment hari ini, pending bills, low stock alerts
   - Receptionist: appointment hari ini, pasien baru hari ini
   - Doctor: appointment saya hari ini
   - Pharmacist: resep pending, low stock alerts
   - Cashier: pending bills, total pembayaran hari ini
   
   /patients → tabel pasien, search, pagination (receptionist, admin)
   /patients/new → form registrasi pasien
   /patients/[id] → detail pasien + history appointment
   
   /appointments → tabel appointment dengan filter status/tanggal
   /appointments/new → form buat appointment (pilih pasien + dokter + waktu)
   /appointments/[id] → detail + action buttons sesuai status
   
   /pharmacy → tabs: Resep Pending | Inventory
   /pharmacy/inventory → tabel stok obat, highlight low stock
   
   /billing → tabel tagihan dengan filter status
   /billing/[id] → detail tagihan + line items + tombol Issue/Pay

4. Shared components:
   - Sidebar dengan navigation role-aware
   - StatusBadge (warna per status)
   - MoneyDisplay (format Rupiah)
   - DataTable (reusable dengan pagination)
   - ConfirmDialog
   - LoadingSkeleton

Desain: clean, professional. Dark sidebar (#1e293b) + light content.
Warna aksen: biru (#2563eb).
```

**Definition of Done:**
- [ ] Login via Keycloak berhasil, session tersimpan
- [ ] Sidebar menampilkan menu sesuai role user yang login
- [ ] Halaman patients CRUD berfungsi (receptionist bisa create, doctor read-only)
- [ ] Halaman appointments: create, lihat detail, complete (doctor)
- [ ] Halaman pharmacy: lihat resep pending, dispense
- [ ] Halaman billing: lihat tagihan, issue, pay
- [ ] Role yang salah akses halaman → redirect atau 403

---

## Phase 6 — Kubernetes (Minikube) Deployment

**Tujuan**: Containerize semua service dan deploy ke Minikube.

**Prompt untuk IBM Bob:**
```
Baca AGENTS.md sebelum mulai.

Phase 5 selesai. Semua service dan frontend sudah running via docker-compose.
Lanjut Phase 6 — Kubernetes deployment dengan Minikube.

1. Dockerfile untuk setiap service:
   - Multi-stage build (sdk → runtime)
   - Base: mcr.microsoft.com/dotnet/aspnet:10.0
   - Setiap service punya Dockerfile sendiri di root folder service

2. Dockerfile untuk frontend (Next.js):
   - Multi-stage: node:20-alpine untuk build, node:20-alpine untuk runtime
   - Standalone output mode

3. Kubernetes manifests di k8s/:
   
   namespace.yaml:
   - Namespace: careops
   
   infrastructure/:
   - postgres.yaml: StatefulSet + Service + PersistentVolumeClaim
   - rabbitmq.yaml: Deployment + Service
   - keycloak.yaml: Deployment + Service + ConfigMap (realm import)
   
   services/ (per service):
   - Deployment (replicas: 1)
   - Service (ClusterIP)
   - ConfigMap untuk environment variables
   - Secret untuk connection strings (base64)
   
   gateway/:
   - api-gateway Deployment + Service (LoadBalancer atau NodePort)
   
   frontend/:
   - frontend Deployment + Service (NodePort untuk akses lokal)
   
   ingress.yaml:
   - careops.local → frontend
   - api.careops.local → api-gateway
   - auth.careops.local → keycloak

4. Helm chart di helm/careops/:
   - Chart.yaml
   - values.yaml (semua configurable values)
   - Templates untuk semua manifests di atas

5. GitHub Actions workflows:
   - build.yml: build Docker images + push ke Docker Hub (atau ghcr.io)
   - deploy.yml: apply Kubernetes manifests

6. README.md lengkap:
   - Prerequisites: Docker, Minikube, kubectl, Helm
   - Step-by-step setup dari nol
   - Demo flow: login sebagai masing-masing role, jalankan end-to-end flow

Gunakan resource limits di setiap Deployment:
requests: cpu: 100m, memory: 128Mi
limits: cpu: 500m, memory: 512Mi
```

**Definition of Done:**
- [ ] Semua Dockerfile build tanpa error
- [ ] `kubectl apply -f k8s/` semua pods Running
- [ ] Bisa akses frontend via Minikube tunnel / NodePort
- [ ] Login Keycloak bekerja di dalam cluster
- [ ] End-to-end flow: register pasien → buat appointment → dispense obat → bayar tagihan
- [ ] `helm install careops ./helm/careops` deploy semua komponen
- [ ] README bisa diikuti dari nol

---

## Tips Penggunaan IBM Bob

1. **Buka AGENTS.md di awal setiap sesi** — Bob tidak ingat sesi sebelumnya
2. **Satu phase per sesi** — jangan gabung beberapa phase sekaligus
3. **Kalau Bob pakai HTTP call antar service**, ingatkan: *"Cek .bob/rules/01-architecture.md — komunikasi antar service harus via RabbitMQ events, bukan HTTP"*
4. **Kalau Bob share database antar service**, ingatkan: *"Cek AGENTS.md bagian Key Technical Rules — database per service, tidak boleh cross-database"*
5. **Kalau Bob pakai Marten**, ingatkan: *"Cek AGENTS.md — project ini TIDAK pakai Marten, gunakan EF Core + Npgsql"*
6. **Untuk iterasi kecil**: *"Baca AGENTS.md. Update [file] untuk [perubahan spesifik]"*
7. **Pattern consumer**: selalu ingatkan Bob bahwa consumer harus idempotent
