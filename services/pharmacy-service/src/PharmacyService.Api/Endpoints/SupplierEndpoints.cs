using MediatR;
using Microsoft.AspNetCore.Mvc;
using PharmacyService.Application.Pharmacy.Commands;
using PharmacyService.Application.Pharmacy.Queries;

namespace PharmacyService.Api.Endpoints;

public static class SupplierEndpoints
{
    public static void MapSupplierEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/pharmacy/suppliers").WithTags("Suppliers");

        group.MapGet("/", async (
            [FromQuery] bool? isActive,
            [FromQuery] int page,
            [FromQuery] int pageSize,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetSuppliersQuery(isActive, page == 0 ? 1 : page, pageSize == 0 ? 50 : pageSize), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("GetSuppliers");

        group.MapPost("/", async (
            CreateSupplierCommand command,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return Results.Created($"/pharmacy/suppliers/{result.Id}", result);
        })
        .RequireAuthorization(p => p.RequireRole("admin", "pharmacist"))
        .WithName("CreateSupplier");

        group.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] UpdateSupplierBody body,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new UpdateSupplierCommand(
                id, body.Name, body.ContactPerson, body.Phone, body.Email, body.Address, body.IsActive), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization(p => p.RequireRole("admin", "pharmacist"))
        .WithName("UpdateSupplier");

        group.MapDelete("/{id:guid}", async (
            Guid id,
            ISender sender,
            CancellationToken ct) =>
        {
            await sender.Send(new DeleteSupplierCommand(id), ct);
            return Results.NoContent();
        })
        .RequireAuthorization(p => p.RequireRole("admin"))
        .WithName("DeleteSupplier");
    }
}

public record UpdateSupplierBody(
    string Name, string? ContactPerson, string? Phone, string? Email, string? Address, bool IsActive);
