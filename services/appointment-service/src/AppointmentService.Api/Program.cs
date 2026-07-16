using AppointmentService.Api.Endpoints;
using AppointmentService.Application.Behaviors;
using AppointmentService.Application.Appointments.Commands;
using AppointmentService.Application.Doctors.Commands;
using AppointmentService.Infrastructure.Persistence;
using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<AppointmentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateAppointmentCommand).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
});

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(CreateAppointmentCommandValidator).Assembly);

// MassTransit + RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        });

        cfg.UseMessageRetry(r => r.Intervals(100, 500, 1000));
        cfg.UseInMemoryOutbox(context);
        cfg.ConfigureEndpoints(context);
    });
});

// JWT Auth (Keycloak)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Keycloak:Authority"];
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            RoleClaimType = ClaimTypes.Role
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;
                if (claimsIdentity == null) return Task.CompletedTask;
                var realmAccess = context.Principal?.FindFirst("realm_access")?.Value;
                if (!string.IsNullOrEmpty(realmAccess))
                {
                    try
                    {
                        var doc = System.Text.Json.JsonDocument.Parse(realmAccess);
                        if (doc.RootElement.TryGetProperty("roles", out var roles))
                            foreach (var role in roles.EnumerateArray())
                            {
                                var r = role.GetString();
                                if (!string.IsNullOrEmpty(r))
                                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, r));
                            }
                    }
                    catch { }
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Health Checks
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString)
    .AddRabbitMQ(rabbitConnectionString: $"amqp://{builder.Configuration["RabbitMQ:Username"] ?? "guest"}:{builder.Configuration["RabbitMQ:Password"] ?? "guest"}@{builder.Configuration["RabbitMQ:Host"] ?? "localhost"}");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Migrate DB on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppointmentDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.ContentType = "application/problem+json";
        var ex = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
        if (ex is not null)
        {
            var (statusCode, title) = ex switch
            {
                AppointmentService.Domain.Exceptions.AppointmentNotFoundException => (404, "Appointment Not Found"),
                AppointmentService.Domain.Exceptions.AppointmentConflictException => (409, "Appointment Conflict"),
                AppointmentService.Domain.Exceptions.AppointmentCannotBeCancelledException => (422, "Cannot Cancel Appointment"),
                AppointmentService.Domain.Exceptions.AppointmentCannotBeCompletedException => (422, "Cannot Complete Appointment"),
                AppointmentService.Domain.Exceptions.DoctorNotAvailableException => (409, "Doctor Not Available"),
                FluentValidation.ValidationException => (400, "Validation Error"),
                _ => (500, "Internal Server Error")
            };
            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = ex.Message
            });
        }
    });
});

app.MapAppointmentEndpoints();
app.MapHealthChecks("/health");

app.Run();
