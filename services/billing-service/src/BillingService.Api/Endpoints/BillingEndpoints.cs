using BillingService.Application.Billing.Commands;
using BillingService.Application.Billing.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BillingService.Api.Endpoints;

public static class BillingEndpoints
{
    public static void MapBillingEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/billing").WithTags("Billing");

        // GET /billing — cashier, admin; patient own only
        group.MapGet("/", async (
            [FromQuery] Guid? patientId,
            [FromQuery] int page, [FromQuery] int pageSize,
            ISender sender, HttpContext httpContext, CancellationToken ct) =>
        {
            var roles = httpContext.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            var effectivePatientId = patientId;

            // patient role: force own data
            if (roles.Contains("patient") && !roles.Contains("admin"))
            {
                var header = httpContext.Request.Headers["X-Patient-Id"].FirstOrDefault();
                if (Guid.TryParse(header, out var pid)) effectivePatientId = pid;
            }

            if (effectivePatientId is null)
                return Results.BadRequest(new { error = "patientId is required." });

            var result = await sender.Send(new GetBillsByPatientQuery(
                effectivePatientId.Value,
                page == 0 ? 1 : page,
                pageSize == 0 ? 20 : pageSize), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("GetBills");

        // GET /billing/summary — cashier, admin
        group.MapGet("/summary", async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetBillsSummaryQuery(), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("GetBillsSummary");

        // GET /billing/{id}
        group.MapGet("/{id:guid}", async (
            Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetBillByIdQuery(id), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("GetBillById");

        // POST /billing/{id}/issue — cashier, admin
        group.MapPost("/{id:guid}/issue", async (
            Guid id, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new IssueBillCommand(id), ct);
            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName("IssueBill");

        // POST /billing/{id}/pay — cashier, admin
        group.MapPost("/{id:guid}/pay", async (
            Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ProcessPaymentCommand(id), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("PayBill");

        // POST /billing/{id}/cancel — cashier, admin
        group.MapPost("/{id:guid}/cancel", async (
            Guid id, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new CancelBillCommand(id), ct);
            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName("CancelBill");
    }
}
