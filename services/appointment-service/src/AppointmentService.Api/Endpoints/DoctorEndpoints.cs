using AppointmentService.Application.Doctors.Commands;
using AppointmentService.Application.Doctors.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentService.Api.Endpoints;

public static class DoctorEndpoints
{
    public static void MapDoctorEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/doctors").WithTags("Doctors");

        // GET /doctors — admin, receptionist, doctor, cashier
        group.MapGet("/", async (
            [FromQuery] string? search,
            [FromQuery] string? specialization,
            [FromQuery] bool? isActive,
            [FromQuery] int page,
            [FromQuery] int pageSize,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetDoctorsQuery(
                search, specialization, isActive,
                page == 0 ? 1 : page,
                pageSize == 0 ? 20 : pageSize), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("GetDoctors");

        // GET /doctors/{id} — admin, receptionist, doctor, cashier
        group.MapGet("/{id:guid}", async (
            Guid id,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetDoctorByIdQuery(id), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("GetDoctorById");

        // POST /doctors — admin only
        group.MapPost("/", async (
            CreateDoctorCommand command,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return Results.Created($"/doctors/{result.Id}", result);
        })
        .RequireAuthorization(policy => policy.RequireRole("admin"))
        .WithName("CreateDoctor");

        // PUT /doctors/{id} — admin only
        group.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] UpdateDoctorBody body,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new UpdateDoctorCommand(
                id, body.Name, body.Specialization, body.LicenseNumber,
                body.Schedule, body.Phone, body.Email, body.IsActive);
            var result = await sender.Send(command, ct);
            return Results.Ok(result);
        })
        .RequireAuthorization(policy => policy.RequireRole("admin"))
        .WithName("UpdateDoctor");

        // DELETE /doctors/{id} — admin only
        group.MapDelete("/{id:guid}", async (
            Guid id,
            ISender sender,
            CancellationToken ct) =>
        {
            await sender.Send(new DeleteDoctorCommand(id), ct);
            return Results.NoContent();
        })
        .RequireAuthorization(policy => policy.RequireRole("admin"))
        .WithName("DeleteDoctor");

        // GET /doctors/{id}/schedule — existing, keep it here too
        group.MapGet("/{id:guid}/schedule", async (
            Guid id,
            [FromQuery] DateOnly? date,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(
                new AppointmentService.Application.Appointments.Queries.GetDoctorScheduleQuery(
                    id, date ?? DateOnly.FromDateTime(DateTime.UtcNow)), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("GetDoctorSchedule");
    }
}

public record UpdateDoctorBody(
    string Name,
    string Specialization,
    string LicenseNumber,
    string Schedule,
    string? Phone,
    string? Email,
    bool IsActive);
