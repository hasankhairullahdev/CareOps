using BillingService.Application.Billing.Commands;
using BillingService.Application.Billing.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BillingService.Api.Endpoints;

public static class PaymentMethodEndpoints
{
    public static void MapPaymentMethodEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/billing/payment-methods").WithTags("PaymentMethods");

        group.MapGet("/", async (
            [FromQuery] bool? isActive,
            ISender sender,
            CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetPaymentMethodsQuery(isActive), ct)))
        .RequireAuthorization()
        .WithName("GetPaymentMethods");

        group.MapPost("/", async (
            CreatePaymentMethodCommand command,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return Results.Created($"/billing/payment-methods/{result.Id}", result);
        })
        .RequireAuthorization(p => p.RequireRole("admin"))
        .WithName("CreatePaymentMethod");

        group.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] UpdatePaymentMethodBody body,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new UpdatePaymentMethodCommand(id, body.Name, body.Description, body.IsActive), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization(p => p.RequireRole("admin"))
        .WithName("UpdatePaymentMethod");

        group.MapDelete("/{id:guid}", async (
            Guid id,
            ISender sender,
            CancellationToken ct) =>
        {
            await sender.Send(new DeletePaymentMethodCommand(id), ct);
            return Results.NoContent();
        })
        .RequireAuthorization(p => p.RequireRole("admin"))
        .WithName("DeletePaymentMethod");
    }
}

public record UpdatePaymentMethodBody(string Name, string? Description, bool IsActive);
