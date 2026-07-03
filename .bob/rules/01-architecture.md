# Architecture Rules

## Microservice Boundaries
- Setiap service punya database sendiri — TIDAK BOLEH cross-database query
- Komunikasi antar service HANYA via RabbitMQ events (async) — tidak boleh direct HTTP call antar service
- Setiap service adalah deployable unit yang independent
- Setiap service bisa di-start tanpa service lain running (loosely coupled)

## Per-Service Internal Architecture
- `Api` → hanya routing dan HTTP concerns
- `Application` → business logic via MediatR (Commands + Queries)
- `Domain` → entities, value objects, business rules — zero external dependencies
- `Infrastructure` → EF Core, MassTransit publishers, external integrations

## CQRS
- Command handler: mutate state via EF Core → publish event via MassTransit
- Query handler: read dari DbContext langsung — boleh pakai projection/select
- Jangan campur command dan query dalam satu handler

## Messaging (MassTransit)
- Publisher ada di Infrastructure layer
- Consumer ada di service yang relevan (folder `Consumers/`)
- Consumer HARUS idempotent — handle duplicate messages dengan graceful
- Gunakan MassTransit Outbox pattern untuk ensure at-least-once delivery
- Nama event: past tense — `PatientRegistered`, bukan `RegisterPatient`

## Auth & Authorization
- Token validation di setiap service — jangan rely ke Gateway saja
- Role check via `[Authorize(Roles = "...")]` atau policy
- Extract user info dari JWT claims: `sub` = userId, `realm_access.roles` = roles
- "own data only" = bandingkan `sub` dari token dengan `patientId` di resource

## Health Checks
- Setiap service WAJIB expose `GET /health`
- Cek: database connectivity, RabbitMQ connectivity
- Gunakan `Microsoft.Extensions.Diagnostics.HealthChecks`
