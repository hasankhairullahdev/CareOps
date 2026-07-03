# .NET Rules (Code Mode)

## Project Setup
- Target framework: `net10.0`
- Nullable reference types: enabled
- Implicit usings: enabled
- TIDAK pakai Marten — gunakan EF Core + Npgsql
- TIDAK pakai Event Sourcing

## EF Core
- DbContext per service: `PatientDbContext`, `AppointmentDbContext`, dst
- Gunakan Fluent API untuk konfigurasi entity — tidak pakai data annotations
- Migration per service — jangan share migration
- Seed data via `modelBuilder.Entity<T>().HasData(...)` atau `IHostedService`
- Gunakan `AsNoTracking()` untuk query-only reads

## MediatR
- Satu file per Command/Query + Handler
- Pipeline behaviors: `ValidationBehavior` → `LoggingBehavior`
- Validasi via FluentValidation

## MassTransit + RabbitMQ
- Setup di `Infrastructure` layer
- Publisher: inject `IPublishEndpoint`, publish di command handler setelah save
- Consumer: implement `IConsumer<TMessage>`
- Aktifkan Outbox: `cfg.UseMessageRetry()` + `cfg.UseInMemoryOutbox()`
- Consumer harus idempotent — cek apakah event sudah diproses sebelumnya

## Keycloak / JWT Auth
- Gunakan `Microsoft.AspNetCore.Authentication.JwtBearer`
- Authority: Keycloak realm URL
- ValidateIssuer: true, ValidateAudience: true
- Extract roles dari `realm_access.roles` claim (bukan standard `roles`)
- Buat custom `IClaimsTransformation` untuk map Keycloak roles ke .NET claims

## YARP (API Gateway)
- Konfigurasi routes di `appsettings.json`
- Middleware: validate JWT → forward ke downstream
- Strip `/api/v1/patients` → forward ke `http://patient-service/patients`
- Tambahkan header `X-User-Id` dan `X-User-Roles` ke downstream request

## Health Checks
```csharp
builder.Services.AddHealthChecks()
    .AddNpgsql(connectionString)
    .AddRabbitMQ(rabbitConnectionString);

app.MapHealthChecks("/health");
```

## Minimal API
- Group per domain: `app.MapGroup("/patients")`
- Gunakan `TypedResults`
- Return `ProblemDetails` untuk errors

## Decimal & Money
- WAJIB `decimal` untuk billing dan kalkulasi harga
- DILARANG `double` atau `float` untuk uang
- Pembulatan rupiah: `Math.Round(value, 0, MidpointRounding.AwayFromZero)`

## Docker
- Setiap service punya `Dockerfile` di root service folder
- Gunakan multi-stage build: `build` stage + `runtime` stage
- Base image: `mcr.microsoft.com/dotnet/aspnet:10.0` untuk runtime
- Build image: `mcr.microsoft.com/dotnet/sdk:10.0`
- EXPOSE port sesuai AGENTS.md
