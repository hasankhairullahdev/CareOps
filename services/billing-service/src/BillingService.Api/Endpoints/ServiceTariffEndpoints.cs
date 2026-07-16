using BillingService.Application.Billing.Commands;
using BillingService.Application.Billing.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BillingService.Api.Endpoints;

public static class ServiceTariffEndpoints
{
    public static void MapServiceTariffEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/billing/tariffs").WithTags("ServiceTariffs");

        // GET /billing/tariffs — admin, cashier
        group.MapGet("/", async (
            [FromQuery] string? search,
            [FromQuery] string? category,
            [FromQuery] bool? isActive,
            [FromQuery] int page,
            [FromQuery] int pageSize,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetServiceTariffsQuery(
                search, category, isActive,
                page == 0 ? 1 : page,
                pageSize == 0 ? 50 : pageSize), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("GetServiceTariffs");

        // GET /billing/tariffs/{id} — admin, cashier
        group.MapGet("/{id:guid}", async (
            Guid id,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetServiceTariffByIdQuery(id), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("GetServiceTariffById");

        // POST /billing/tariffs — admin only
        group.MapPost("/", async (
            CreateServiceTariffCommand command,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return Results.Created($"/billing/tariffs/{result.Id}", result);
        })
        .RequireAuthorization(policy => policy.RequireRole("admin"))
        .WithName("CreateServiceTariff");

        // PUT /billing/tariffs/{id} — admin only
        group.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] UpdateTariffBody body,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new UpdateServiceTariffCommand(
                id, body.ServiceName, body.Category, body.Price, body.Description, body.IsActive);
            var result = await sender.Send(command, ct);
            return Results.Ok(result);
        })
        .RequireAuthorization(policy => policy.RequireRole("admin"))
        .WithName("UpdateServiceTariff");

        // DELETE /billing/tariffs/{id} — admin only
        group.MapDelete("/{id:guid}", async (
            Guid id,
            ISender sender,
            CancellationToken ct) =>
        {
            await sender.Send(new DeleteServiceTariffCommand(id), ct);
            return Results.NoContent();
        })
        .RequireAuthorization(policy => policy.RequireRole("admin"))
        .WithName("DeleteServiceTariff");
    }
}

public record UpdateTariffBody(
    string ServiceName,
    string Category,
    decimal Price,
    string? Description,
    bool IsActive);
