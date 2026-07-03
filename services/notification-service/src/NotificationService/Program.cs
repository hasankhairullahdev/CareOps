using MassTransit;
using NotificationService.Consumers;
using NotificationService.Storage;

var builder = WebApplication.CreateBuilder(args);

// In-memory notification store
builder.Services.AddSingleton<INotificationStore, InMemoryNotificationStore>();

// MassTransit + RabbitMQ — all consumers registered
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<PatientRegisteredConsumer>();
    x.AddConsumer<AppointmentCreatedConsumer>();
    x.AddConsumer<AppointmentCancelledConsumer>();
    x.AddConsumer<PrescriptionDispensedConsumer>();
    x.AddConsumer<BillGeneratedConsumer>();
    x.AddConsumer<BillPaidConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        });

        cfg.UseMessageRetry(r => r.Intervals(100, 500, 1000));
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddHealthChecks()
    .AddRabbitMQ(rabbitConnectionString: $"amqp://{builder.Configuration["RabbitMQ:Username"] ?? "guest"}:{builder.Configuration["RabbitMQ:Password"] ?? "guest"}@{builder.Configuration["RabbitMQ:Host"] ?? "localhost"}");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }

// GET /notifications/{userId}
app.MapGet("/notifications/{userId}", (string userId, INotificationStore store) =>
{
    var notifications = store.GetForUser(userId);
    return Results.Ok(notifications);
})
.WithTags("Notifications")
.WithName("GetNotifications");

app.MapHealthChecks("/health");
app.Run();
