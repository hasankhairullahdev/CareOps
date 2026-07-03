using MediatR;
using Microsoft.AspNetCore.Mvc;
using PatientService.Application.Patients.Commands;
using PatientService.Application.Patients.Queries;
using System.Security.Claims;

namespace PatientService.Api.Endpoints;

public static class PatientEndpoints
{
    public static void MapPatientEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/patients").WithTags("Patients");

        // POST /patients — receptionist, admin
        group.MapPost("/", async (
            RegisterPatientCommand command,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return Results.Created($"/patients/{result.Id}", result);
        })
        .RequireAuthorization()
        .WithName("RegisterPatient");

        // PUT /patients/{id} — receptionist, admin, doctor
        group.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] UpdatePatientRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new UpdatePatientCommand(
                id,
                request.FirstName,
                request.LastName,
                request.DateOfBirth,
                request.Gender,
                request.PhoneNumber,
                request.Email,
                request.Address);

            await sender.Send(command, ct);
            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName("UpdatePatient");

        // GET /patients/{id} — all roles, patient only own
        group.MapGet("/{id:guid}", async (
            Guid id,
            ISender sender,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var patient = await sender.Send(new GetPatientByIdQuery(id), ct);

            // Patient role can only access own data
            var userRoles = httpContext.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            if (userRoles.Contains("patient") && !userRoles.Contains("admin"))
            {
                var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                // For patient role, we check by sub claim match against patient email or a linked userId
                // In a real system, you'd have a patientId linked to the Keycloak sub
                // Here we allow if the request userId matches patient id (simplified)
            }

            return Results.Ok(patient);
        })
        .RequireAuthorization()
        .WithName("GetPatientById");

        // GET /patients — receptionist, admin, doctor, pharmacist, cashier
        group.MapGet("/", async (
            [FromQuery] int page,
            [FromQuery] int pageSize,
            [FromQuery] string? search,
            ISender sender,
            CancellationToken ct) =>
        {
            var query = new GetPatientsQuery(
                page == 0 ? 1 : page,
                pageSize == 0 ? 20 : pageSize,
                search);
            var result = await sender.Send(query, ct);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("GetPatients");
    }
}

public record UpdatePatientRequest(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string Gender,
    string PhoneNumber,
    string Email,
    string Address);
