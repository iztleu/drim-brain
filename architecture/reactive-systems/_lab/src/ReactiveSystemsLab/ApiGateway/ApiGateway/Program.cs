using System.Reflection;
using ApiGateway.Features.Notifications;
using ApiGateway.Features.Notifications.HostedServices;
using ApiGateway.Features.Notifications.Services;
using Microsoft.Extensions.Http.Resilience;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddMediatR(cfg => cfg
    .RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

builder.Services.AddSignalR();

builder.Services.AddSingleton<NotificationStream>();
builder.Services.AddHostedService<NotificationStreamHostedService>();

builder.Services.AddGrpcClient<NotificationService.Client.Notifications.NotificationsClient>(options =>
{
    options.Address = new Uri("http://notification-service");
});

var app = builder.Build();

app.UseStaticFiles();

app.MapHub<NotificationHub>("/notificationHub");

app.MapDefaultEndpoints();

app.Run();
