# CareOps — Hospital Management System

A production-grade hospital management system built on **.NET 10 microservices**, **Next.js 14**, **MassTransit + RabbitMQ**, **Keycloak**, and **PostgreSQL**. Deployable locally via Podman Compose or to Kubernetes (Minikube / AKS) via Helm.

---

## Architecture

```
Browser / Next.js (port 3000)
    └─> API Gateway — YARP (port 5000)
         ├─> Validate JWT (Keycloak)
         ├─> Forward X-User-Id + X-User-Roles headers
         └─> Route to downstream service
              ├─> patient-service      :5001
              ├─> appointment-service  :5002
              ├─> pharmacy-service     :5003
              ├─> billing-service      :5004
              └─> notification-service :5005

Service → RabbitMQ (publish domain event)
    └─> notification-service (all events → in-memory notifications)
    └─> billing-service      (AppointmentCreated, PrescriptionDispensed, AppointmentCancelled)
    └─> pharmacy-service     (PrescriptionCreated)
```

### Services

| Service | Port | Database | Responsibility |
|---|---|---|---|
| patient-service | 5001 | patient_db | Patient registration & medical records |
| appointment-service | 5002 | appointment_db | Doctor schedules & bookings |
| pharmacy-service | 5003 | pharmacy_db | Medicine inventory & prescription dispensing |
| billing-service | 5004 | billing_db | Bills & payments |
| notification-service | 5005 | — | Event consumer, in-memory notifications |
| api-gateway | 5000 | — | YARP reverse proxy + JWT auth |
| keycloak | 8080 | keycloak_db | Identity provider (OpenID Connect) |

### Domain Events

| Event | Publisher | Consumers |
|---|---|---|
| PatientRegistered | patient-service | notification-service |
| AppointmentCreated | appointment-service | notification-service, billing-service |
| AppointmentCancelled | appointment-service | notification-service, billing-service |
| AppointmentCompleted | appointment-service | notification-service |
| PrescriptionCreated | appointment-service | pharmacy-service |
| PrescriptionDispensed | pharmacy-service | billing-service, notification-service |
| BillGenerated | billing-service | notification-service |
| BillPaid | billing-service | notification-service |

---

## Prerequisites

### Local Development (Podman Compose)
- [Podman](https://podman.io/) v5+ with podman-compose v1.5+
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js](https://nodejs.org/) v20+
- PowerShell 7+

### Kubernetes Deployment
- [Minikube](https://minikube.sigs.k8s.io/) v1.32+
- [kubectl](https://kubernetes.io/docs/tasks/tools/) v1.29+
- [Helm](https://helm.sh/) v3.14+
- [Docker](https://www.docker.com/) (for building images)

---

## Quick Start — Local Development

### 1. Clone the repository

```bash
git clone https://github.com/hasankhairullahdev/CareOps.git
cd CareOps
```

### 2. Start infrastructure

```powershell
podman compose -f docker-compose.infra.yml up -d
```

Wait ~30s for Keycloak to import the realm, then verify:
- PostgreSQL: `localhost:5432`
- RabbitMQ Management: http://localhost:15672 (guest / guest)
- Keycloak: http://localhost:8080 (admin / admin)

### 3. Start all services

```powershell
.\scripts\dev-start.ps1
```

This opens separate terminals for each service. Or start manually:

```powershell
# Terminal 1 — patient-service
cd services/patient-service
dotnet run --project src/PatientService.Api

# Terminal 2 — appointment-service
cd services/appointment-service
dotnet run --project src/AppointmentService.Api

# Terminal 3 — pharmacy-service
cd services/pharmacy-service
dotnet run --project src/PharmacyService.Api

# Terminal 4 — billing-service
cd services/billing-service
dotnet run --project src/BillingService.Api

# Terminal 5 — notification-service
cd services/notification-service
dotnet run --project src/NotificationService

# Terminal 6 — api-gateway
cd gateway/HospitalGateway
dotnet run

# Terminal 7 — frontend
cd frontend
npm install
npm run dev
```

### 4. Configure frontend environment

```bash
cp frontend/.env.example frontend/.env.local
```

Edit `.env.local`:
```env
NEXTAUTH_URL=http://localhost:3000
NEXTAUTH_SECRET=any-random-secret-32-chars
KEYCLOAK_CLIENT_ID=careops-frontend
KEYCLOAK_CLIENT_SECRET=
KEYCLOAK_ISSUER=http://localhost:8080/realms/careops
NEXT_PUBLIC_API_URL=http://localhost:5000
```

### 5. Open the app

Navigate to http://localhost:3000

---

## Test Users

All passwords: `Admin123!`

| Email | Role | Access |
|---|---|---|
| admin@hospital.com | admin | Full access |
| reception@hospital.com | receptionist | Patients + Appointments |
| doctor@hospital.com | doctor | Appointments (own) + Prescriptions |
| pharmacist@hospital.com | pharmacist | Pharmacy + Inventory |
| cashier@hospital.com | cashier | Billing |
| patient@hospital.com | patient | Own data only |

---

## Demo Flow (End-to-End)

```
1. Login as receptionist → register a new patient → POST /api/patients
2. Login as receptionist → create appointment for that patient → POST /api/appointments
   ↳ billing-service auto-creates a Draft bill (Rp 150,000 consultation fee)
   ↳ notification-service logs "appointment created"
3. Login as doctor → complete appointment → POST /api/appointments/{id}/complete
4. Login as doctor → create prescription → POST /api/appointments/{id}/prescriptions
   ↳ pharmacy-service receives PrescriptionCreated → status Pending
5. Login as pharmacist → dispense prescription → POST /api/pharmacy/dispense
   ↳ medicine stock deducted
   ↳ billing-service adds medicine line items to the bill
6. Login as cashier → issue the bill → POST /api/billing/{id}/issue
7. Login as cashier → process payment → POST /api/billing/{id}/pay
   ↳ notification-service logs "payment received"
```

---

## API Reference

All endpoints go through the API Gateway at `http://localhost:5000`.

### Patients
| Method | Endpoint | Roles |
|---|---|---|
| POST | /api/patients | admin, receptionist |
| GET | /api/patients | admin, receptionist, doctor, pharmacist, cashier |
| GET | /api/patients/{id} | admin, receptionist, doctor, pharmacist, cashier, patient (own) |
| PUT | /api/patients/{id} | admin, receptionist |

### Appointments
| Method | Endpoint | Roles |
|---|---|---|
| POST | /api/appointments | admin, receptionist |
| GET | /api/appointments | admin, receptionist, doctor (own), cashier, patient (own) |
| GET | /api/appointments/{id} | admin, receptionist, doctor, cashier, patient (own) |
| POST | /api/appointments/{id}/complete | admin, doctor |
| POST | /api/appointments/{id}/cancel | admin, receptionist, doctor |
| POST | /api/appointments/{id}/prescriptions | admin, doctor |
| GET | /api/doctors | all authenticated |

### Pharmacy
| Method | Endpoint | Roles |
|---|---|---|
| GET | /api/pharmacy/inventory | admin, pharmacist |
| POST | /api/pharmacy/stock | admin, pharmacist |
| POST | /api/pharmacy/dispense | admin, pharmacist |
| GET | /api/pharmacy/prescriptions/pending | admin, pharmacist |

### Billing
| Method | Endpoint | Roles |
|---|---|---|
| GET | /api/billing | admin, cashier, patient (own) |
| GET | /api/billing/{id} | admin, cashier, patient (own) |
| POST | /api/billing/{id}/issue | admin, cashier |
| POST | /api/billing/{id}/pay | admin, cashier |
| POST | /api/billing/{id}/cancel | admin, cashier |

---

## Kubernetes Deployment (Minikube)

### 1. Start Minikube

```bash
minikube start --cpus=4 --memory=8192 --driver=docker
minikube addons enable ingress
```

### 2. Configure hosts file

Add to `/etc/hosts` (Linux/Mac) or `C:\Windows\System32\drivers\etc\hosts` (Windows):
```
$(minikube ip)  careops.local api.careops.local auth.careops.local
```

Or use `minikube tunnel` and add `127.0.0.1` instead.

### 3. Build and push images

> Set `GITHUB_OWNER` to your GitHub username (lowercase).

```bash
GITHUB_OWNER=hasankhairullahdev
REGISTRY=ghcr.io/$GITHUB_OWNER/careops

# Login to GHCR
echo $GITHUB_TOKEN | docker login ghcr.io -u $GITHUB_OWNER --password-stdin

# Build & push each service
docker build -t $REGISTRY/patient-service:latest services/patient-service && docker push $REGISTRY/patient-service:latest
docker build -t $REGISTRY/appointment-service:latest services/appointment-service && docker push $REGISTRY/appointment-service:latest
docker build -t $REGISTRY/pharmacy-service:latest services/pharmacy-service && docker push $REGISTRY/pharmacy-service:latest
docker build -t $REGISTRY/billing-service:latest services/billing-service && docker push $REGISTRY/billing-service:latest
docker build -t $REGISTRY/notification-service:latest services/notification-service && docker push $REGISTRY/notification-service:latest
docker build -t $REGISTRY/api-gateway:latest gateway/HospitalGateway && docker push $REGISTRY/api-gateway:latest
docker build -t $REGISTRY/frontend:latest frontend && docker push $REGISTRY/frontend:latest
```

### 4. Create the Keycloak realm ConfigMap

```bash
kubectl create namespace careops --dry-run=client -o yaml | kubectl apply -f -
kubectl create configmap keycloak-realm \
  --from-file=realm-export.json=gateway/keycloak/realm-export.json \
  -n careops --dry-run=client -o yaml | kubectl apply -f -
```

### 5a. Deploy with plain kubectl

```bash
kubectl apply -f k8s/namespace.yaml
kubectl apply -f k8s/infrastructure/
kubectl apply -f k8s/services/
kubectl apply -f k8s/gateway/
kubectl apply -f k8s/frontend/
kubectl apply -f k8s/ingress.yaml
```

Wait for all pods to be ready:
```bash
kubectl get pods -n careops -w
```

### 5b. Deploy with Helm (alternative)

```bash
helm upgrade --install careops ./helm/careops \
  --namespace careops \
  --create-namespace \
  --set global.imageRegistry=ghcr.io/$GITHUB_OWNER/careops \
  --set global.imageTag=latest \
  --set frontend.nextauthSecret=<your-secret>
```

### 6. Access the app

```bash
# Option A: NodePort (no ingress needed)
minikube service frontend -n careops --url    # frontend
minikube service api-gateway -n careops --url  # API

# Option B: Ingress
minikube tunnel  # run in separate terminal
# then open http://careops.local
```

---

## GitHub Actions CI/CD

The repo includes two workflows:

### `build.yml` — triggered on every push to `master`
1. Builds all 7 Docker images (matrix strategy — runs in parallel)
2. Pushes to GitHub Container Registry (`ghcr.io`)
3. Tags: `latest` (master), `sha-<short>`, branch name

### `deploy.yml` — triggered after `build.yml` succeeds
1. Configures kubectl using `KUBECONFIG_B64` secret
2. Runs `helm upgrade --install careops`
3. Verifies all rollouts succeed

**Required secrets** in GitHub repo settings:
| Secret | Description |
|---|---|
| `KUBECONFIG_B64` | Base64-encoded kubeconfig (`cat ~/.kube/config \| base64`) |
| `NEXTAUTH_SECRET` | Random 32+ char string for NextAuth |

---

## Health Checks

Every service exposes `GET /health`:

```bash
curl http://localhost:5001/health  # patient-service
curl http://localhost:5002/health  # appointment-service
curl http://localhost:5003/health  # pharmacy-service
curl http://localhost:5004/health  # billing-service
curl http://localhost:5005/health  # notification-service
curl http://localhost:5000/health  # api-gateway
```

Or use the smoke-test script:
```powershell
.\scripts\dev-test.ps1
```

---

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | .NET 10 Web API, MediatR, FluentValidation |
| ORM | EF Core + Npgsql |
| Messaging | MassTransit + RabbitMQ (Outbox pattern) |
| Auth | Keycloak (OpenID Connect), JWT Bearer |
| API Gateway | YARP (Yet Another Reverse Proxy) |
| Frontend | Next.js 14 (App Router), NextAuth v5, TanStack Query v5 |
| Styling | Tailwind CSS |
| Database | PostgreSQL 16 |
| Containers | Docker / Podman |
| Orchestration | Kubernetes (Minikube / AKS) |
| Package Manager | Helm v3 |
| CI/CD | GitHub Actions |

---

## Project Structure

```
careops/
├── services/
│   ├── patient-service/          # .NET 10, port 5001
│   ├── appointment-service/      # .NET 10, port 5002
│   ├── pharmacy-service/         # .NET 10, port 5003
│   ├── billing-service/          # .NET 10, port 5004
│   └── notification-service/     # .NET 10, port 5005
├── gateway/
│   ├── HospitalGateway/          # YARP, port 5000
│   └── keycloak/realm-export.json
├── frontend/                     # Next.js 14, port 3000
├── k8s/                          # Plain Kubernetes manifests
│   ├── namespace.yaml
│   ├── infrastructure/           # postgres, rabbitmq, keycloak
│   ├── services/                 # 5 backend services
│   ├── gateway/                  # api-gateway
│   ├── frontend/
│   └── ingress.yaml
├── helm/careops/                 # Helm chart
├── scripts/                      # dev-start, dev-stop, dev-test, dev-token
├── docker-compose.yml            # Full local stack
├── docker-compose.infra.yml      # Infrastructure only
└── .github/workflows/            # build.yml, deploy.yml
```
