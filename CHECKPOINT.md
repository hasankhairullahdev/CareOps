# CHECKPOINT.md — CareOps Single Source of Truth

> **Baca file ini sebelum melakukan APAPUN.**
> File ini adalah checkpoint resmi yang merekam state codebase, keputusan teknis, dan rencana kerja berikutnya.
> Update file ini setiap kali ada perubahan signifikan.

---

## 1. Project Overview

**CareOps** — Hospital Management System berbasis microservice architecture.

| Aspek | Pilihan |
|---|---|
| Backend | .NET 10 Web API, Minimal API |
| Frontend | Next.js 14 (App Router), TypeScript, Tailwind CSS, shadcn/ui |
| Messaging | MassTransit + RabbitMQ |
| Auth | Keycloak (OpenID Connect / OAuth 2.0) + NextAuth v5 |
| Database | PostgreSQL — **database per service**, via EF Core + Npgsql |
| API Gateway | YARP (Yet Another Reverse Proxy) |
| Container | **Podman** (bukan Docker!) — versi 5.8.2 + podman-compose 1.5.0 |
| CI/CD | GitHub Actions → build.yml + deploy.yml |
| K8s | Minikube local → AKS |
| Git Remote | https://github.com/hasankhairullahdev/CareOps.git (branch: **master**) |

---

## 2. Services & Ports

| Service | Port | Database | Tanggung Jawab |
|---|---|---|---|
| `patient-service` | 5001 | `patient_db` | Registrasi & rekam medis pasien |
| `appointment-service` | 5002 | `appointment_db` | Jadwal dokter & booking |
| `pharmacy-service` | 5003 | `pharmacy_db` | Stok obat & dispensing resep |
| `billing-service` | 5004 | `billing_db` | Tagihan & pembayaran |
| `notification-service` | 5005 | — | Consumer events, kirim notifikasi |
| `api-gateway` | 5000 | — | YARP reverse proxy + JWT validation |
| `identity (Keycloak)` | 8080 | `keycloak_db` | Identity Provider |
| `frontend (Next.js)` | 3000 | — | Web UI |

---

## 3. Keputusan Teknis Permanen (JANGAN UBAH)

1. **TIDAK pakai Event Sourcing** — gunakan EF Core + PostgreSQL standard
2. **TIDAK pakai Marten** — gunakan `Microsoft.EntityFrameworkCore` + `Npgsql`
3. **Database per service** — DILARANG cross-database query
4. **Komunikasi antar service HANYA via RabbitMQ events** — TIDAK BOLEH direct HTTP call antar service
5. **JWT validation di setiap service** — tidak rely ke Gateway saja, validasi via Keycloak JWKS
6. **Guid PKs** — semua primary key menggunakan `Guid`, kecuali `BloodType.Id` (int, enum-like)
7. **Decimal for money** — semua billing pakai `decimal`
8. **MassTransit Outbox pattern** — untuk at-least-once delivery
9. **Consumer harus idempotent** — handle duplicate messages dengan graceful
10. **Health check `/health`** — setiap service wajib expose
11. **Satu .sln per service** — jangan campur solution antar service
12. **Podman** bukan Docker — semua container command pakai `podman`/`podman-compose`
13. **next.config.ts** — TIDAK SUPPORTED di Next.js 14, gunakan `next.config.mjs`

---

## 4. Naming Conventions

| Hal | Konvensi |
|---|---|
| Classes, records, enums | `PascalCase` |
| Methods, properties | `PascalCase` |
| Local variables, parameters | `camelCase` |
| Private fields | `_camelCase` |
| Interfaces | prefix `I` → `IPatientRepository` |
| Events (MassTransit) | Past tense → `PatientRegistered`, `AppointmentCreated` |
| Commands | Imperative → `RegisterPatientCommand`, `CreateDoctorCommand` |
| Queries | Noun → `GetPatientByIdQuery`, `GetDoctorsQuery` |
| Command+Handler | Satu file bersama |
| Namespace-level functions | **DILARANG** — gunakan static class + method |

---

## 5. State Implementasi

### ✅ SELESAI — Phase 1–6 (Core Infrastructure)

| Phase | Konten |
|---|---|
| Phase 1 | Infrastructure (postgres, rabbitmq, keycloak) + patient-service |
| Phase 2 | appointment-service + API Gateway (YARP) |
| Phase 3 | pharmacy-service + notification-service |
| Phase 4 | billing-service |
| Phase 5 | Next.js 14 Frontend (PreClinic design) |
| Phase 6 | Kubernetes + Helm + CI/CD |

### ✅ SELESAI — Fase A: Master Data Prioritas Tinggi

| Item | Service | Status |
|---|---|---|
| Doctor CRUD + Schedule | appointment-service | ✅ Done |
| Service Tariff CRUD | billing-service | ✅ Done |

### ✅ SELESAI — Fase B: Master Data Prioritas Sedang

| Item | Service | Status |
|---|---|---|
| Blood Type (seeded, read-only) | patient-service | ✅ Done |
| Allergy Type CRUD | patient-service | ✅ Done |
| Supplier CRUD | pharmacy-service | ✅ Done |
| Payment Method CRUD (seeded) | billing-service | ✅ Done |

### ⏳ BELUM — Fase C: Master Data Prioritas Rendah (hardcode for now)

| Item | Service | Rencana |
|---|---|---|
| Specialization | appointment-service | Hardcode / enum |
| Room | appointment-service | Hardcode |
| Medicine Category | pharmacy-service | Hardcode / string field |
| Unit | pharmacy-service | Hardcode / string field |
| Insurance | billing-service | Belum diimplementasi |
| Notification Templates | notification-service | Belum diimplementasi |

---

## 6. File Map Lengkap per Service

### 6.1 patient-service (port 5001, DB: patient_db)

```
src/
├── PatientService.Api/
│   ├── Program.cs                          ← JWT auth, MediatR, MassTransit, health checks
│   └── Endpoints/
│       ├── PatientEndpoints.cs             ← GET/POST/PUT /patients
│       └── LookupEndpoints.cs              ← GET /blood-types, CRUD /allergy-types
├── PatientService.Application/
│   ├── Patients/
│   │   ├── Commands/
│   │   │   ├── RegisterPatientCommand.cs
│   │   │   ├── UpdatePatientCommand.cs
│   │   │   ├── PatientCommandValidators.cs
│   │   │   └── LookupCommands.cs           ← Create/Update/Delete AllergyType
│   │   └── Queries/
│   │       ├── GetPatientsQuery.cs
│   │       ├── GetPatientByIdQuery.cs
│   │       └── LookupQueries.cs            ← GetBloodTypesQuery, GetAllergyTypesQuery
│   └── Behaviors/
│       ├── ValidationBehavior.cs
│       └── LoggingBehavior.cs
├── PatientService.Domain/
│   ├── Entities/
│   │   ├── Patient.cs
│   │   └── LookupEntities.cs              ← BloodType, AllergyType
│   ├── ValueObjects/
│   │   └── MedicalRecordNumber.cs
│   └── Exceptions/
│       ├── PatientNotFoundException.cs
│       └── DuplicateMedicalRecordException.cs
└── PatientService.Infrastructure/
    └── Persistence/
        ├── PatientDbContext.cs             ← Patients, BloodTypes, AllergyTypes
        └── Migrations/
            ├── 20240101000000_InitialCreate.cs
            └── 20240102000000_AddLookupTables.cs  ← BloodTypes + AllergyTypes
```

**Domain Events yang dipublish:**
- `PatientRegistered` → consumed by: notification-service

---

### 6.2 appointment-service (port 5002, DB: appointment_db)

```
src/
├── AppointmentService.Api/
│   ├── Program.cs
│   └── Endpoints/
│       ├── AppointmentEndpoints.cs         ← CRUD /appointments + cancel/complete/prescriptions
│       └── DoctorEndpoints.cs              ← CRUD /doctors + GET /doctors/{id}/schedule
├── AppointmentService.Application/
│   ├── Appointments/
│   │   ├── Commands/
│   │   │   ├── CreateAppointmentCommand.cs
│   │   │   ├── CancelAppointmentCommand.cs
│   │   │   ├── CompleteAppointmentCommand.cs
│   │   │   ├── CreatePrescriptionCommand.cs
│   │   │   └── AppointmentCommandValidators.cs
│   │   └── Queries/
│   │       ├── GetAppointmentsQuery.cs     ← AppointmentDto defined here
│   │       ├── GetAppointmentByIdQuery.cs
│   │       └── GetDoctorScheduleQuery.cs
│   ├── Doctors/
│   │   ├── Commands/
│   │   │   └── DoctorCommands.cs          ← Create/Update/Delete Doctor + validators
│   │   └── Queries/
│   │       └── DoctorQueries.cs           ← GetDoctorsQuery, GetDoctorByIdQuery, DoctorDto
│   └── Behaviors/
│       └── PipelineBehaviors.cs
├── AppointmentService.Domain/
│   ├── Entities/
│   │   ├── Appointment.cs
│   │   ├── Doctor.cs                      ← Id, Name, Specialization, LicenseNumber, Schedule, Phone, Email, IsActive, CreatedAt
│   │   ├── Prescription.cs
│   │   └── PrescriptionItem.cs
│   ├── Enums/
│   │   └── AppointmentStatus.cs           ← Scheduled, InProgress, Completed, Cancelled
│   └── Exceptions/
│       ├── AppointmentNotFoundException.cs
│       ├── AppointmentConflictException.cs
│       ├── AppointmentCannotBeCancelledException.cs
│       ├── AppointmentCannotBeCompletedException.cs
│       └── DoctorNotAvailableException.cs
└── AppointmentService.Infrastructure/
    └── Persistence/
        ├── AppointmentDbContext.cs         ← Appointments, Doctors (seeded 3), Prescriptions, PrescriptionItems
        └── Migrations/
            ├── 20240101000000_InitialCreate.cs
            └── 20240102000000_AddDoctorContactFields.cs  ← Phone, Email, IsActive, CreatedAt
```

**Seed data:**
- `d0000000-0000-0000-0000-000000000001` → Dr. Andi Wirawan (General Practice)
- `d0000000-0000-0000-0000-000000000002` → Dr. Sari Kusuma (Internal Medicine)
- `d0000000-0000-0000-0000-000000000003` → Dr. Bima Prasetyo (Pediatrics)

**Domain Events:**
- `AppointmentCreated` → consumed by: notification-service, billing-service
- `AppointmentCancelled` → consumed by: notification-service, billing-service
- `PrescriptionCreated` → consumed by: pharmacy-service

---

### 6.3 pharmacy-service (port 5003, DB: pharmacy_db)

```
src/
├── PharmacyService.Api/
│   ├── Program.cs
│   └── Endpoints/
│       ├── PharmacyEndpoints.cs            ← inventory, dispense, stock
│       └── SupplierEndpoints.cs            ← CRUD /pharmacy/suppliers
├── PharmacyService.Application/
│   ├── Pharmacy/
│   │   ├── Commands/
│   │   │   ├── DispensePrescriptionCommand.cs
│   │   │   ├── AddStockCommand.cs
│   │   │   ├── PharmacyCommandValidators.cs
│   │   │   └── SupplierCommands.cs         ← Create/Update/Delete Supplier
│   │   ├── Queries/
│   │   │   ├── GetInventoryQuery.cs
│   │   │   ├── GetPrescriptionQuery.cs
│   │   │   └── SupplierQueries.cs          ← GetSuppliersQuery
│   │   └── Consumers/
│   │       └── PrescriptionCreatedConsumer.cs
│   └── Behaviors/
│       └── PipelineBehaviors.cs
├── PharmacyService.Domain/
│   ├── Entities/
│   │   ├── Medicine.cs
│   │   ├── Prescription.cs
│   │   ├── PrescriptionItem.cs
│   │   ├── StockMovement.cs
│   │   └── Supplier.cs                    ← BARU
│   ├── Enums/
│   │   └── PharmacyEnums.cs
│   └── Exceptions/
│       └── PharmacyExceptions.cs
└── PharmacyService.Infrastructure/
    └── Persistence/
        ├── PharmacyDbContext.cs            ← Medicines (seeded 5), Prescriptions, PrescriptionItems, StockMovements, Suppliers
        └── Migrations/
            ├── 20240101000000_InitialCreate.cs
            └── 20240102000000_AddSuppliers.cs
```

**Domain Events:**
- `PrescriptionDispensed` → consumed by: billing-service, notification-service

---

### 6.4 billing-service (port 5004, DB: billing_db)

```
src/
├── BillingService.Api/
│   ├── Program.cs
│   └── Endpoints/
│       ├── BillingEndpoints.cs             ← CRUD /billing + issue/pay/cancel
│       ├── ServiceTariffEndpoints.cs       ← CRUD /billing/tariffs
│       └── PaymentMethodEndpoints.cs       ← CRUD /billing/payment-methods
├── BillingService.Application/
│   ├── Billing/
│   │   ├── Commands/
│   │   │   ├── BillingCommands.cs          ← IssueBill, ProcessPayment, CancelBill
│   │   │   ├── ServiceTariffCommands.cs    ← Create/Update/Delete ServiceTariff
│   │   │   └── PaymentMethodCommands.cs    ← Create/Update/Delete PaymentMethod
│   │   ├── Queries/
│   │   │   ├── BillingQueries.cs           ← GetBillById, GetBillsByPatient, GetBillsSummary
│   │   │   ├── ServiceTariffQueries.cs     ← GetServiceTariffs, GetServiceTariffById
│   │   │   └── PaymentMethodQueries.cs     ← GetPaymentMethods
│   │   └── Consumers/
│   │       └── BillingConsumers.cs         ← AppointmentCreated (lookup tariff from DB!), AppointmentCancelled, PrescriptionDispensed
│   └── Behaviors/
│       └── PipelineBehaviors.cs
├── BillingService.Domain/
│   ├── Entities/
│   │   ├── Bill.cs
│   │   ├── BillLineItem.cs
│   │   ├── ServiceTariff.cs
│   │   └── PaymentMethod.cs               ← BARU
│   ├── Enums/
│   │   └── BillStatus.cs
│   └── Exceptions/
│       └── BillingExceptions.cs           ← BillNotFound, BillAlreadyPaid, BillNotIssued, ServiceTariffNotFound
└── BillingService.Infrastructure/
    └── Persistence/
        ├── BillingDbContext.cs             ← Bills, BillLineItems, ServiceTariffs (seeded), PaymentMethods (seeded)
        └── Migrations/
            ├── 20240101000000_InitialCreate.cs
            ├── 20240102000000_AddServiceTariffs.cs
            └── 20240103000000_AddPaymentMethods.cs
```

**Seed data:**
- ServiceTariff: `b0000000-0000-0000-0000-000000000001` → Biaya Konsultasi Umum (Rp 150.000, Category: Consultation)
- PaymentMethod: Cash, Transfer Bank, BPJS, Kartu Debit/Kredit

**⚠️ PENTING:** `AppointmentCreatedConsumer` lookup tariff dari DB (`Category == "Consultation" && IsActive`), fallback Rp 150.000 kalau tidak ada.

---

### 6.5 notification-service (port 5005)

```
src/NotificationService/
├── Program.cs
├── Consumers/                             ← MassTransit consumers untuk semua events
└── Channels/                              ← Email, in-app channels
```

---

### 6.6 API Gateway (port 5000)

```
gateway/HospitalGateway/
├── Program.cs
└── appsettings.json                       ← YARP routes config
```

**YARP Routes (prefix `/api/...` → service path):**

| YARP Route | Cluster | Path |
|---|---|---|
| `/api/patients/**` | patient-service | `/patients/**` |
| `/api/blood-types/**` | patient-service | `/blood-types/**` |
| `/api/allergy-types/**` | patient-service | `/allergy-types/**` |
| `/api/appointments/**` | appointment-service | `/appointments/**` |
| `/api/doctors/**` | appointment-service | `/doctors/**` |
| `/api/pharmacy/suppliers/**` | pharmacy-service | `/pharmacy/suppliers/**` |
| `/api/pharmacy/**` | pharmacy-service | `/pharmacy/**` |
| `/api/billing/payment-methods/**` | billing-service | `/billing/payment-methods/**` |
| `/api/billing/tariffs/**` | billing-service | `/billing/tariffs/**` |
| `/api/billing/**` | billing-service | `/billing/**` |
| `/api/notifications/**` | notification-service | `/notifications/**` |

> ⚠️ **URUTAN PENTING** — route lebih spesifik (`/billing/payment-methods`, `/billing/tariffs`) harus dideklarasikan SEBELUM route umum (`/billing/`).

---

### 6.7 Frontend (port 3000)

**Framework:** Next.js 14, App Router, TypeScript, Tailwind CSS, shadcn/ui
**Config file:** `next.config.mjs` (bukan `.ts`!)
**Design system:** PreClinic style — sidebar putih, primary `#2E37A4`, accent teal `#00D3C7`

```
frontend/
├── app/
│   ├── layout.tsx                          ← root layout, <Providers>
│   ├── page.tsx                            ← redirect ke /dashboard
│   ├── (auth)/
│   │   └── login/page.tsx
│   └── (dashboard)/
│       ├── layout.tsx                      ← <Sidebar> + <Topbar> wrapper (div, BUKAN html/body!)
│       ├── dashboard/page.tsx              ← stat cards real API
│       ├── patients/
│       │   ├── page.tsx                    ← list + search
│       │   ├── new/page.tsx
│       │   └── [id]/page.tsx
│       ├── appointments/
│       │   ├── page.tsx
│       │   ├── new/page.tsx
│       │   └── [id]/page.tsx
│       ├── doctors/                        ← BARU (Fase A)
│       │   ├── page.tsx                    ← list + admin actions
│       │   ├── new/page.tsx
│       │   └── [id]/edit/page.tsx
│       ├── pharmacy/
│       │   ├── page.tsx
│       │   └── dispense/[id]/page.tsx
│       ├── billing/
│       │   ├── page.tsx
│       │   └── [id]/page.tsx
│       └── admin/
│           ├── tariffs/page.tsx            ← ServiceTariff CRUD (Fase A)
│           ├── payment-methods/page.tsx    ← PaymentMethod CRUD (Fase B)
│           ├── suppliers/page.tsx          ← Supplier CRUD (Fase B)
│           └── allergy-types/page.tsx      ← AllergyType CRUD (Fase B)
├── components/
│   ├── Sidebar.tsx                         ← grouped nav, role-based visibility
│   ├── Topbar.tsx
│   ├── Providers.tsx                       ← SessionProvider wrapper
│   ├── StatusBadge.tsx
│   ├── AppointmentActions.tsx
│   ├── BillActions.tsx
│   └── DispenseButton.tsx
├── lib/
│   ├── api.ts                              ← semua API calls (patientsApi, doctorsApi, tariffsApi, dll)
│   ├── auth.ts                             ← NextAuth v5 config + JWT roles decode
│   ├── roles.ts                            ← hasRole(), hasAnyRole(), UserRole type
│   ├── types.ts                            ← semua TypeScript interfaces
│   └── utils.ts                            ← cn() helper
└── types/
    └── next-auth.d.ts                      ← Session type augmentation (accessToken, idToken, roles)
```

---

## 7. API Reference per Service

### patient-service endpoints

| Method | Path | Roles | Keterangan |
|---|---|---|---|
| POST | `/patients` | admin, receptionist | Register pasien baru |
| GET | `/patients` | admin, receptionist, doctor, pharmacist, cashier | List + search |
| GET | `/patients/{id}` | semua | Get by ID |
| PUT | `/patients/{id}` | admin, receptionist | Update |
| GET | `/blood-types` | semua | Read-only, seeded |
| GET | `/allergy-types` | semua | List (filter isActive) |
| POST | `/allergy-types` | admin | Create |
| PUT | `/allergy-types/{id}` | admin | Update |
| DELETE | `/allergy-types/{id}` | admin | Delete |

### appointment-service endpoints

| Method | Path | Roles | Keterangan |
|---|---|---|---|
| GET | `/doctors` | semua | List + search + filter |
| GET | `/doctors/{id}` | semua | Get by ID |
| POST | `/doctors` | admin | Create |
| PUT | `/doctors/{id}` | admin | Update |
| DELETE | `/doctors/{id}` | admin | Delete (cek FK appointment) |
| GET | `/doctors/{id}/schedule` | semua | Schedule + appointments by date |
| POST | `/appointments` | admin, receptionist | Buat appointment |
| GET | `/appointments` | semua | List (patient role: filtered) |
| GET | `/appointments/{id}` | semua | Get by ID |
| POST | `/appointments/{id}/cancel` | admin, receptionist, doctor | Cancel |
| POST | `/appointments/{id}/complete` | admin, doctor | Complete |
| POST | `/appointments/{id}/prescriptions` | admin, doctor | Buat resep |

### pharmacy-service endpoints

| Method | Path | Roles | Keterangan |
|---|---|---|---|
| GET | `/pharmacy/inventory` | admin, pharmacist | Inventory list |
| POST | `/pharmacy/dispense` | admin, pharmacist | Dispense prescription |
| POST | `/pharmacy/stock` | admin, pharmacist | Tambah stok |
| GET | `/pharmacy/prescriptions/pending` | admin, pharmacist | Pending prescriptions |
| GET | `/pharmacy/prescriptions/{id}` | admin, pharmacist | Get prescription |
| GET | `/pharmacy/suppliers` | admin, pharmacist | List suppliers |
| POST | `/pharmacy/suppliers` | admin, pharmacist | Create |
| PUT | `/pharmacy/suppliers/{id}` | admin, pharmacist | Update |
| DELETE | `/pharmacy/suppliers/{id}` | admin | Delete |

### billing-service endpoints

| Method | Path | Roles | Keterangan |
|---|---|---|---|
| GET | `/billing` | admin, cashier, patient | List (patient filtered) |
| GET | `/billing/{id}` | admin, cashier, patient | Get by ID |
| POST | `/billing/{id}/issue` | admin, cashier | Issue bill |
| POST | `/billing/{id}/pay` | admin, cashier | Process payment |
| POST | `/billing/{id}/cancel` | admin, cashier | Cancel |
| GET | `/billing/summary` | admin, cashier | Dashboard summary |
| GET | `/billing/tariffs` | admin, cashier | List tariffs |
| GET | `/billing/tariffs/{id}` | admin, cashier | Get tariff |
| POST | `/billing/tariffs` | admin | Create |
| PUT | `/billing/tariffs/{id}` | admin | Update |
| DELETE | `/billing/tariffs/{id}` | admin | Delete |
| GET | `/billing/payment-methods` | semua | List |
| POST | `/billing/payment-methods` | admin | Create |
| PUT | `/billing/payment-methods/{id}` | admin | Update |
| DELETE | `/billing/payment-methods/{id}` | admin | Delete |

---

## 8. Domain Events Map

| Event | Publisher | Consumers | Keterangan |
|---|---|---|---|
| `PatientRegistered` | patient-service | notification-service | Kirim welcome notif |
| `AppointmentCreated` | appointment-service | notification-service, billing-service | Buat bill (lookup tariff dari DB) |
| `AppointmentCancelled` | appointment-service | notification-service, billing-service | Cancel bill |
| `PrescriptionCreated` | appointment-service | pharmacy-service | Buat prescription di pharmacy |
| `PrescriptionDispensed` | pharmacy-service | billing-service, notification-service | Tambah line item obat ke bill |
| `BillGenerated` | billing-service | notification-service | Notif bill issued |
| `BillPaid` | billing-service | notification-service | Notif paid |

---

## 9. Auth & Authorization

### Keycloak Setup
- **Realm:** `careops`
- **Client frontend:** `careops-frontend` (public, PKCE)
- **Client gateway:** `api-gateway` (confidential)
- **JWT claims:** `realm_access.roles` array

### Roles
| Role | Akses |
|---|---|
| `admin` | Full access semua fitur |
| `receptionist` | Patients (create/read), Appointments (create/read) |
| `doctor` | Patients (read), Appointments (read, prescribe), Doctors (read) |
| `pharmacist` | Pharmacy (full), Suppliers (create/edit), Patients (read) |
| `cashier` | Billing (full), Appointments (read), Patients (read) |
| `patient` | Own data only |

### Token Flow (NextAuth v5)
```
1. Redirect ke Keycloak login
2. Auth code → NextAuth callback
3. JWT callback: decode accessToken → extract realm_access.roles → simpan ke token
4. Session callback: token.roles → session.roles
5. Fallback: kalau roles kosong, decode JWT accessToken manual
6. Logout: signOut → events.signOut → Keycloak end_session endpoint + id_token_hint
```

### Auth Gotchas (sudah diperbaiki)
- `AUTH_SECRET` (bukan `NEXTAUTH_SECRET`) untuk NextAuth v5 + `trustHost: true`
- `next-auth.d.ts` augment Session dengan `accessToken`, `idToken`, `roles`
- Roles decode dari `realm_access.roles` di jwt callback, fallback decode manual

---

## 10. Database Seed Data

| Service | Table | Seed |
|---|---|---|
| appointment-service | Doctors | 3 dokter (Andi, Sari, Bima) |
| billing-service | ServiceTariffs | 1 tariff (Konsultasi Umum Rp 150.000) |
| billing-service | PaymentMethods | Cash, Transfer Bank, BPJS, Kartu Debit/Kredit |
| patient-service | BloodTypes | A+, A-, B+, B-, AB+, AB-, O+, O- |
| pharmacy-service | Medicines | 5 obat (Paracetamol, Amoxicillin, Omeprazole, Cetirizine, Metformin) |

**Seed ID conventions:**
- Doctor IDs: `d0000000-0000-0000-0000-000000000001` → `003`
- ServiceTariff ID: `b0000000-0000-0000-0000-000000000001`
- PaymentMethod IDs: `c0000000-0000-0000-0000-000000000001` → `004`
- Medicine IDs: `00000000-0000-0000-0000-000000000001` → `005`

---

## 11. Frontend lib/api.ts — API Objects

| Export | Endpoints yang dicover |
|---|---|
| `patientsApi` | list, get, create, update |
| `appointmentsApi` | list, get, create, cancel, complete, createPrescription, getDoctorSchedule |
| `doctorsApi` | list, get, create, update, delete |
| `pharmacyApi` | inventory, pendingPrescriptions, getPrescription, dispense, addStock |
| `tariffsApi` | list, get, create, update, delete |
| `paymentMethodsApi` | list, create, update, delete |
| `lookupsApi` | bloodTypes, allergyTypes, createAllergyType, updateAllergyType, deleteAllergyType |
| `suppliersApi` | list, create, update, delete |
| `billingApi` | list, get, summary, issue, pay, cancel |

---

## 12. lib/types.ts — TypeScript Interfaces

`Patient`, `PaginatedResult<T>`, `Appointment`, `AppointmentStatus`, `Doctor`, `PaginatedDoctorsResult`, `ServiceTariff`, `PaginatedTariffsResult`, `Medicine`, `Prescription`, `PrescriptionItem`, `InventoryResult`, `Bill`, `BillLineItem`, `BillsSummary`, `BloodType`, `AllergyType`, `Supplier`, `GetSuppliersResult`, `PaymentMethod`

---

## 13. Sidebar Navigation Groups

```
Main Menu
  └── Dashboard (semua role)

Clinic
  ├── Patients (admin, receptionist, doctor, pharmacist, cashier)
  ├── Appointments (admin, receptionist, doctor, cashier, patient)
  ├── Doctors (admin, receptionist, doctor, cashier)
  └── Pharmacy (admin, pharmacist)

Finance
  └── Billing (admin, cashier, patient)

Administration  [admin only, kecuali Suppliers]
  ├── Service Tariffs (admin)
  ├── Payment Methods (admin)
  ├── Suppliers (admin, pharmacist)
  ├── Allergy Types (admin)
  └── Admin (admin)
```

---

## 14. Migration History

### patient-service
| Timestamp | Name |
|---|---|
| 20240101000000 | InitialCreate — Patients table |
| 20240102000000 | AddLookupTables — BloodTypes + AllergyTypes |

### appointment-service
| Timestamp | Name |
|---|---|
| 20240101000000 | InitialCreate — Doctors, Appointments, Prescriptions, PrescriptionItems |
| 20240102000000 | AddDoctorContactFields — Phone, Email, IsActive, CreatedAt ke Doctors |

### pharmacy-service
| Timestamp | Name |
|---|---|
| 20240101000000 | InitialCreate — Medicines, Prescriptions, PrescriptionItems, StockMovements |
| 20240102000000 | AddSuppliers — Suppliers table |

### billing-service
| Timestamp | Name |
|---|---|
| 20240101000000 | InitialCreate — Bills, BillLineItems |
| 20240102000000 | AddServiceTariffs — ServiceTariffs table + seed |
| 20240103000000 | AddPaymentMethods — PaymentMethods table + seed |

---

## 15. Environment Variables

```env
# Per service (contoh patient-service)
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=Host=localhost;Database=patient_db;Username=postgres;Password=postgres
Keycloak__Authority=http://localhost:8080/realms/careops
Keycloak__Audience=api-gateway
RabbitMQ__Host=localhost
RabbitMQ__Username=guest
RabbitMQ__Password=guest

# API Gateway
Keycloak__Authority=http://localhost:8080/realms/careops
Keycloak__ClientId=api-gateway
Keycloak__ClientSecret=your-secret

# Frontend (.env.local)
AUTH_SECRET=your-auth-secret           # NextAuth v5 — BUKAN NEXTAUTH_SECRET
NEXTAUTH_SECRET=your-auth-secret       # backward compat — isi sama dengan AUTH_SECRET
KEYCLOAK_CLIENT_ID=careops-frontend
KEYCLOAK_CLIENT_SECRET=
KEYCLOAK_ISSUER=http://localhost:8080/realms/careops
NEXT_PUBLIC_API_URL=http://localhost:5000
```

---

## 16. Known Gotchas & Fixes

| Issue | Fix |
|---|---|
| `next.config.ts` tidak support di Next.js 14 | Gunakan `next.config.mjs` |
| `postcss.config.js` missing | Tambahkan di root frontend/ |
| `(dashboard)/layout.tsx` nested `<html><body>` | Ganti ke `<div>` |
| `(auth)/layout.tsx` nested `<html><body>` | Ganti ke `<>fragment</>` |
| NextAuth v5 `AUTH_SECRET` | Isi kedua var — `AUTH_SECRET` dan `NEXTAUTH_SECRET` |
| Roles kosong setelah page refresh | Decode JWT accessToken manual di jwt callback sebagai fallback |
| Logout tidak clear Keycloak session | Call Keycloak `end_session` endpoint dengan `id_token_hint` di events.signOut |
| Seed data di migration — harus include ALL columns | Anonymous object harus punya semua field non-nullable |
| Namespace-level function di C# | DILARANG — gunakan `internal static class Mapper { static ... }` |
| BloodType PK | int (bukan Guid) — karena fixed enum-like data |

---

## 17. Rencana Kerja Selanjutnya

### ⏳ Next Up — Integrasi & Polish

1. **Patient ↔ BloodType/AllergyType linkage**
   - Tambah `BloodTypeId?` (int) dan `AllergyTypes` (many-to-many) ke Patient entity
   - Update form register/edit pasien di frontend untuk pilih blood type + allergy

2. **Medicine CRUD frontend**
   - Saat ini Medicine hanya bisa dilihat di inventory (pharmacy page)
   - Admin/pharmacist perlu bisa tambah/edit/hapus medicine dari UI

3. **Admin page `/admin`**
   - Saat ini link `/admin` di sidebar belum ada halaman
   - Bisa dijadikan overview/dashboard untuk admin

4. **Bill ↔ PaymentMethod linkage**
   - Saat ini PaymentMethod sudah ada tapi tidak digunakan di Bill
   - Perlu tambah `PaymentMethodId?` FK ke Bill, pilih saat pay

5. **Appointment + Doctor detail di frontend**
   - `/appointments/new` perlu dropdown doctor dari `GET /doctors`
   - `/appointments/[id]` perlu tampilkan detail dokter

6. **Fase C Master Data** (low priority, saat ini hardcode)
   - Room management
   - Insurance
   - Notification templates

---

## 18. Build Validation

Jalankan sebelum commit:
```powershell
# Build semua services
dotnet build services/patient-service/src/PatientService.Api/PatientService.Api.csproj -v q
dotnet build services/appointment-service/src/AppointmentService.Api/AppointmentService.Api.csproj -v q
dotnet build services/pharmacy-service/src/PharmacyService.Api/PharmacyService.Api.csproj -v q
dotnet build services/billing-service/src/BillingService.Api/BillingService.Api.csproj -v q

# TypeCheck frontend (dalam folder frontend/)
npx tsc --noEmit
```

**Last verified build:** ✅ 0 errors, 0 warnings — semua 4 services (appointment, billing, pharmacy, patient)
