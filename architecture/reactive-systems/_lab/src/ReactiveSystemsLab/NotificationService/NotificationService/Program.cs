using NotificationService.Database;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<NotificationDbContext>("NotificationServiceDb");

var app = builder.Build();

app.MapDefaultEndpoints();

app.Run();
