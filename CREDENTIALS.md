# CareOps — Login Credentials

## 🌐 Frontend (http://localhost:3000)

| Email                    | Password  | Role          | Akses                                      |
|--------------------------|-----------|---------------|--------------------------------------------|
| admin@hospital.com       | Admin123! | admin         | Full access semua fitur                    |
| reception@hospital.com   | Admin123! | receptionist  | Patients + Appointments (create/read)      |
| doctor@hospital.com      | Admin123! | doctor        | Appointments (own) + Buat Resep            |
| pharmacist@hospital.com  | Admin123! | pharmacist    | Pharmacy + Inventory                       |
| cashier@hospital.com     | Admin123! | cashier       | Billing (issue/pay/cancel)                 |
| patient@hospital.com     | Admin123! | patient       | Data sendiri saja                          |

---

## 🔧 Admin Tools

| Service         | URL                     | Username | Password |
|-----------------|-------------------------|----------|----------|
| Keycloak Admin  | http://localhost:8080   | admin    | admin    |
| RabbitMQ UI     | http://localhost:15672  | guest    | guest    |
| PostgreSQL      | localhost:5432          | postgres | postgres |

---

## 🚀 Cara Start

```powershell
# Start semua (infra + 6 service + frontend)
.\scripts\dev-start.ps1

# Stop semua
.\scripts\dev-start.ps1 -Stop

# Start infra saja (postgres, rabbitmq, keycloak)
.\scripts\dev-start.ps1 -Infra
```

## 📌 URL Services

| Service           | URL                              |
|-------------------|----------------------------------|
| Frontend          | http://localhost:3000            |
| API Gateway       | http://localhost:5000            |
| Patient Service   | http://localhost:5001/swagger    |
| Appointment Svc   | http://localhost:5002/swagger    |
| Pharmacy Svc      | http://localhost:5003/swagger    |
| Billing Svc       | http://localhost:5004/swagger    |
| Notification Svc  | http://localhost:5005/swagger    |
