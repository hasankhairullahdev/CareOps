# Coding Conventions

## Naming
- Classes, records, enums: `PascalCase`
- Methods, properties: `PascalCase`
- Local variables, parameters: `camelCase`
- Private fields: `_camelCase`
- Interfaces: prefix `I` → `IPatientRepository`
- Events: past tense → `PatientRegistered`, `AppointmentCreated`
- Commands: imperative → `RegisterPatientCommand`, `CreateAppointmentCommand`
- Queries: noun → `GetPatientByIdQuery`, `GetAppointmentsQuery`

## File Organization
- Satu class per file
- Nama file = nama class
- Command + Handler dalam satu file

## Error Handling
- Domain exceptions untuk business rule violations
- Return `ProblemDetails` (RFC 7807) dari API layer
- Global exception middleware di setiap service
- Jangan swallow exceptions

## Async/Await
- Semua I/O harus async
- Suffix `Async` untuk semua async methods
- Selalu pass `CancellationToken`
- Jangan `.Result` atau `.Wait()`

## Testing
- Nama test: `MethodName_Scenario_ExpectedResult`
- Unit test untuk domain logic dan handlers
- Integration test dengan Testcontainers (PostgreSQL + RabbitMQ)
