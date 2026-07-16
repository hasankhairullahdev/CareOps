using MediatR;
using Microsoft.AspNetCore.Mvc;
using PatientService.Application.Patients.Commands;
using PatientService.Application.Patients.Queries;

namespace PatientService.Api.Endpoints;

public static class LookupEndpoints
{
    public static void MapLookupEndpoints(this WebApplication app)
    {
        // ── Blood Types (read-only, seeded) ───────────────────────────────────
        var btGroup = app.MapGroup("/blood-types").WithTags("Lookups");

        btGroup.MapGet("/", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetBloodTypesQuery(), ct)))
        .RequireAuthorization()
        .WithName("GetBloodTypes");

        // ── Allergy Types ─────────────────────────────────────────────────────
        var atGroup = app.MapGroup("/allergy-types").WithTags("Lookups");

        atGroup.MapGet("/", async (
            [FromQuery] bool? isActive,
            ISender sender,
            CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetAllergyTypesQuery(isActive), ct)))
        .RequireAuthorization()
        .WithName("GetAllergyTypes");

        atGroup.MapPost("/", async (
            CreateAllergyTypeCommand command,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return Results.Created($"/allergy-types/{result.Id}", result);
        })
        .RequireAuthorization(p => p.RequireRole("admin"))
        .WithName("CreateAllergyType");

        atGroup.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] UpdateAllergyTypeBody body,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new UpdateAllergyTypeCommand(id, body.Name, body.Description, body.IsActive), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization(p => p.RequireRole("admin"))
        .WithName("UpdateAllergyType");

        atGroup.MapDelete("/{id:guid}", async (
            Guid id,
            ISender sender,
            CancellationToken ct) =>
        {
            await sender.Send(new DeleteAllergyTypeCommand(id), ct);
            return Results.NoContent();
        })
        .RequireAuthorization(p => p.RequireRole("admin"))
        .WithName("DeleteAllergyType");
    }
}

public record UpdateAllergyTypeBody(string Name, string? Description, bool IsActive);
