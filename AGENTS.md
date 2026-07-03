# AGENTS.md — CareOps

## Project Overview

CareOps berbasis microservice architecture untuk mengelola operasional rumah sakit: registrasi pasien, appointment, pharmacy, billing, dan notifikasi.

- **Architecture**: Microservices + API Gateway
- **Backend**: .NET 10 Web API (per service)
- **Frontend**: Next.js 14 (App Router)
- **Messaging**: MassTransit + RabbitMQ
- **Auth**: Keycloak (OpenID Connect / OAuth 2.0)
- **Database**: PostgreSQL (database per service, via EF Core)
- **API Gateway**: YARP (Yet Another Reverse Proxy)
- **Container**: Docker + Kubernetes (Minikube local, AKS later)

---

## Services Overview

| Service | Port | Database | Responsibility |
|---|---|---|---|
| `patient-service` | 5001 | `patient_db` | Registrasi & rekam medis pasien |
| `appointment-service` | 5002 | `appointment_db` | Jadwal dokter & booking |
| `pharmacy-service` | 5003 | `pharmacy_db` | Stok obat & dispensing resep |
| `billing-service` | 5004 | `billing_db` | Tagihan & pembayaran |
| `notification-service` | 5005 | — | Consumer events, kirim notifikasi |
| `api-gateway` | 5000 | — | YARP reverse proxy + auth middleware |
| `identity` | 8080 | `keycloak_db` | Keycloak identity provider |

---

## Repository Structure

```
careops/
├── services/
│   ├── patient-service/
│   │   ├── src/
│   │   │   ├── PatientService.Api/
│   │   │   │   ├── Endpoints/
│   │   │   │   └── Program.cs
│   │   │   ├── PatientService.Application/
│   │   │   │   ├── Patients/
│   │   │   │   │   ├── Commands/
│   │   │   │   │   └── Queries/
│   │   │   │   └── Behaviors/
│   │   │   ├── PatientService.Domain/
│   │   │   │   ├── Entities/
│   │   │   │   ├── ValueObjects/
│   │   │   │   └── Exceptions/
│   │   │   └── PatientService.Infrastructure/
│   │   │       ├── Persistence/
│   │   │       │   ├── PatientDbContext.cs
│   │   │       │   └── Migrations/
│   │   │       └── Messaging/
│   │   │           └── Events/
│   │   ├── tests/
│   │   └── Dockerfile
│   │
│   ├── appointment-service/     # Struktur sama seperti patient-service
│   ├── pharmacy-service/        # Struktur sama seperti patient-service
│   ├── billing-service/         # Struktur sama seperti patient-service
│   └── notification-service/
│       ├── src/
│       │   └── NotificationService/
│       │       ├── Consumers/   # MassTransit consumers
│       │       ├── Channels/    # Email, in-app
│       │       └── Program.cs
│       └── Dockerfile
│
├── gateway/
│   ├── HospitalGateway/
│   │   ├── Program.cs
│   │   ├── appsettings.json     # YARP route config
│   │   └── Dockerfile
│   └── keycloak/
│       └── realm-export.json    # Keycloak realm config
│
├── frontend/
│   ├── app/
│   │   ├── (auth)/
│   │   │   └── login/
│   │   ├── (dashboard)/
│   │   │   ├── patients/
│   │   │   ├── appointments/
│   │   │   ├── pharmacy/
│   │   │   ├── billing/
│   │   │   └── admin/
│   │   ├── layout.tsx
│   │   └── page.tsx
│   ├── components/
│   │   ├── ui/                  # Shadcn components
│   │   ├── patients/
│   │   ├── appointments/
│   │   ├── pharmacy/
│   │   └── billing/
│   └── lib/
│       ├── api.ts               # Typed API client
│       └── auth.ts              # Keycloak integration (next-auth)
│
├── k8s/
│   ├── namespace.yaml
│   ├── services/
│   │   ├── patient-service.yaml
│   │   ├── appointment-service.yaml
│   │   ├── pharmacy-service.yaml
│   │   ├── billing-service.yaml
│   │   └── notification-service.yaml
│   ├── gateway/
│   │   └── api-gateway.yaml
│   ├── infrastructure/
│   │   ├── postgres.yaml
│   │   ├── rabbitmq.yaml
│   │   └── keycloak.yaml
│   └── ingress.yaml
│
├── helm/
│   └── careops/
│       ├── Chart.yaml
│       ├── values.yaml
│       └── templates/
│
├── .github/
│   └── workflows/
│       ├── build.yml            # Build & push Docker images
│       └── deploy.yml           # Deploy ke Minikube / AKS
│
├── docker-compose.yml           # Local dev (tanpa K8s)
├── docker-compose.infra.yml     # Hanya infrastructure (PostgreSQL, RabbitMQ, Keycloak)
├── AGENTS.md
└── PHASE_PROMPTS.md
```

---

## Architecture Flow

```
Browser / Next.js
    └─> API Gateway (YARP :5000)
         ├─> Validate JWT (Keycloak)
         ├─> Check Role (dari JWT claims)
         └─> Route ke service yang tepat
              ├─> patient-service:5001
              ├─> appointment-service:5002
              ├─> pharmacy-service:5003
              └─> billing-service:5004

Service → RabbitMQ (publish event)
    └─> notification-service (consume event → kirim notifikasi)
    └─> billing-service (consume AppointmentCreated → buat billing record)
    └─> pharmacy-service (consume PrescriptionCreated → proses stok)
```

---

## Domain Events (via MassTransit + RabbitMQ)

| Event | Publisher | Consumers |
|---|---|---|
| `PatientRegistered` | patient-service | notification-service |
| `AppointmentCreated` | appointment-service | notification-service, billing-service |
| `AppointmentCancelled` | appointment-service | notification-service, billing-service |
| `PrescriptionCreated` | appointment-service | pharmacy-service |
| `PrescriptionDispensed` | pharmacy-service | billing-service, notification-service |
| `BillGenerated` | billing-service | notification-service |
| `BillPaid` | billing-service | notification-service |

---

## User Roles & Access Control

### Roles (defined in Keycloak)
- `admin` — full access semua service
- `receptionist` — patient + appointment (create/read)
- `doctor` — patient (read/update), appointment (read), pharmacy (create resep)
- `pharmacist` — pharmacy (full), patient (read)
- `cashier` — billing (full), appointment (read), patient (read)
- `patient` — own data only (patient, appointment, billing)

### Role → Endpoint Access

| Endpoint | Admin | Receptionist | Doctor | Pharmacist | Cashier | Patient |
|---|---|---|---|---|---|---|
| POST /patients | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| GET /patients/{id} | ✅ | ✅ | ✅ own | ✅ | ✅ | ✅ own |
| POST /appointments | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| GET /appointments | ✅ | ✅ | ✅ own | ❌ | ✅ | ✅ own |
| POST /prescriptions | ✅ | ❌ | ✅ | ❌ | ❌ | ❌ |
| POST /pharmacy/dispense | ✅ | ❌ | ❌ | ✅ | ❌ | ❌ |
| GET /pharmacy/inventory | ✅ | ❌ | ❌ | ✅ | ❌ | ❌ |
| GET /billing/{id} | ✅ | ❌ | ❌ | ❌ | ✅ | ✅ own |
| POST /billing/pay | ✅ | ❌ | ❌ | ❌ | ✅ | ❌ |

---

## Keycloak Configuration

- Realm: `careops`
- Client: `careops-frontend` (public, PKCE)
- Client: `api-gateway` (confidential, untuk token introspection)
- Roles: admin, receptionist, doctor, pharmacist, cashier, patient
- JWT claims: `realm_access.roles` array

### Token Flow
```
1. Next.js redirect ke Keycloak login page
2. User login → Keycloak return authorization code
3. Next.js tukar code → access token + refresh token
4. Setiap API call: Authorization: Bearer <access_token>
5. YARP forward token ke downstream service
6. Service validasi token signature via Keycloak JWKS endpoint
7. Service extract roles dari JWT claims
```

---

## Per-Service Architecture Pattern

Setiap service mengikuti pattern yang sama:

```
Api/Endpoints          → Minimal API, terima HTTP request
Application/Commands   → MediatR command handlers
Application/Queries    → MediatR query handlers
Application/Behaviors  → Validation, Logging pipeline
Domain/Entities        → EF Core entities, business rules
Infrastructure/        → DbContext, Repositories, MassTransit publishers
```

**TIDAK pakai Event Sourcing** — gunakan EF Core + PostgreSQL standard.
**TIDAK pakai Marten** — gunakan `Microsoft.EntityFrameworkCore` + `Npgsql`.

---

## Key Technical Rules

- **Database per service** — TIDAK BOLEH satu service query database service lain
- **Async messaging** — komunikasi antar service HANYA via RabbitMQ events, TIDAK BOLEH direct HTTP call antar service (kecuali query sederhana via API Gateway)
- **JWT validation** — setiap service validasi token sendiri via Keycloak JWKS, tidak depend ke Gateway
- **Idempotent consumers** — MassTransit consumer harus idempotent (pakai outbox pattern)
- **Health checks** — setiap service expose `/health` endpoint untuk Kubernetes liveness probe
- **Guid PKs** — semua primary key menggunakan `Guid`
- **Decimal for money** — semua kalkulasi billing pakai `decimal`

---

## Environment Variables

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

# Frontend
NEXTAUTH_URL=http://localhost:3000
NEXTAUTH_SECRET=your-nextauth-secret
KEYCLOAK_CLIENT_ID=careops-frontend
KEYCLOAK_CLIENT_SECRET=
KEYCLOAK_ISSUER=http://localhost:8080/realms/careops
NEXT_PUBLIC_API_URL=http://localhost:5000
```

---

## Notes untuk IBM Bob

- **Baca file ini sebelum generate kode apapun**
- Setiap service adalah solution .NET terpisah — jangan campur antar service
- TIDAK pakai Event Sourcing, TIDAK pakai Marten — gunakan EF Core + PostgreSQL
- Komunikasi antar service HANYA via RabbitMQ events — jangan HTTP call langsung
- Setiap service harus punya `/health` endpoint
- Role authorization di-enforce di setiap service dari JWT claims, bukan hanya di Gateway
- Untuk mock data development, seed via `IHostedService` atau EF Core `HasData()`
