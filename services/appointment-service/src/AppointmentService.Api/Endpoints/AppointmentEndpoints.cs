using AppointmentService.Application.Appointments.Commands;
using AppointmentService.Application.Appointments.Queries;
using AppointmentService.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AppointmentService.Api.Endpoints;

public static class AppointmentEndpoints
{
    public static void MapAppointmentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/appointments").WithTags("Appointments");

        // POST /appointments — receptionist, admin
        group.MapPost("/", async (
            CreateAppointmentCommand command,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return Results.Created($"/appointments/{result.Id}", result);
        })
        .RequireAuthorization()
        .WithName("CreateAppointment");

        // GET /appointments — all roles (patient filtered in handler at gateway level)
        group.MapGet("/", async (
            [FromQuery] Guid? doctorId,
            [FromQuery] Guid? patientId,
            [FromQuery] DateOnly? date,
            [FromQuery] AppointmentStatus? status,
            [FromQuery] int page,
            [FromQuery] int pageSize,
            ISender sender,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            var userRoles = httpContext.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Patient role: force filter to their own patientId if header provided
            var effectivePatientId = patientId;
            if (userRoles.Contains("patient") && !userRoles.Contains("admin"))
            {
                var patientIdHeader = httpContext.Request.Headers["X-Patient-Id"].FirstOrDefault();
                if (Guid.TryParse(patientIdHeader, out var pid))
                    effectivePatientId = pid;
            }

            var query = new GetAppointmentsQuery(
                doctorId,
                effectivePatientId,
                date,
                status,
                page == 0 ? 1 : page,
                pageSize == 0 ? 20 : pageSize);

            var result = await sender.Send(query, ct);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("GetAppointments");

        // GET /appointments/{id}
        group.MapGet("/{id:guid}", async (
            Guid id,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetAppointmentByIdQuery(id), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("GetAppointmentById");

        // POST /appointments/{id}/cancel — receptionist, doctor, admin
        group.MapPost("/{id:guid}/cancel", async (
            Guid id,
            [FromBody] CancelRequest? request,
            ISender sender,
            CancellationToken ct) =>
        {
            await sender.Send(new CancelAppointmentCommand(id, request?.Reason), ct);
            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName("CancelAppointment");

        // POST /appointments/{id}/complete — doctor, admin
        group.MapPost("/{id:guid}/complete", async (
            Guid id,
            ISender sender,
            CancellationToken ct) =>
        {
            await sender.Send(new CompleteAppointmentCommand(id), ct);
            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName("CompleteAppointment");

        // POST /appointments/{id}/prescriptions — doctor, admin
        group.MapPost("/{id:guid}/prescriptions", async (
            Guid id,
            [FromBody] CreatePrescriptionRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new CreatePrescriptionCommand(id, request.Items
                .Select(i => new PrescriptionItemRequest(i.MedicineName, i.Quantity, i.Dosage, i.Instructions))
                .ToList());
            var result = await sender.Send(command, ct);
            return Results.Created($"/appointments/{id}/prescriptions/{result.PrescriptionId}", result);
        })
        .RequireAuthorization()
        .WithName("CreatePrescription");

    }
}

public record CancelRequest(string? Reason);

public record CreatePrescriptionRequest(List<PrescriptionItemRequestDto> Items);
public record PrescriptionItemRequestDto(
    string MedicineName,
    int Quantity,
    string Dosage,
    string Instructions);
