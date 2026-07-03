using MediatR;
using Microsoft.AspNetCore.Mvc;
using PharmacyService.Application.Pharmacy.Commands;
using PharmacyService.Application.Pharmacy.Queries;

namespace PharmacyService.Api.Endpoints;

public static class PharmacyEndpoints
{
    public static void MapPharmacyEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/pharmacy").WithTags("Pharmacy");

        // POST /pharmacy/dispense — pharmacist, admin
        group.MapPost("/dispense", async (
            DispensePrescriptionCommand command,
            ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("DispensePrescription");

        // GET /pharmacy/prescriptions/pending — pharmacist, admin
        group.MapGet("/prescriptions/pending", async (
            [FromQuery] int page, [FromQuery] int pageSize,
            ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetPendingPrescriptionsQuery(
                page == 0 ? 1 : page, pageSize == 0 ? 20 : pageSize), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("GetPendingPrescriptions");

        // GET /pharmacy/prescriptions/{id}
        group.MapGet("/prescriptions/{id:guid}", async (
            Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetPrescriptionByIdQuery(id), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("GetPrescriptionById");

        // GET /pharmacy/inventory — pharmacist, admin
        group.MapGet("/inventory", async (
            [FromQuery] bool? lowStockOnly,
            [FromQuery] bool? expiringSoonOnly,
            [FromQuery] int page, [FromQuery] int pageSize,
            ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetInventoryQuery(
                lowStockOnly, expiringSoonOnly,
                page == 0 ? 1 : page, pageSize == 0 ? 50 : pageSize), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("GetInventory");

        // POST /pharmacy/stock — pharmacist, admin
        group.MapPost("/stock", async (
            AddStockCommand command,
            ISender sender, CancellationToken ct) =>
        {
            await sender.Send(command, ct);
            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName("AddStock");
    }
}
