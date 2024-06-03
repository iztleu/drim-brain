using System.Reflection;
using ApiGateway.Clients;
using ApiGateway.Common.Metrics;
using ApiGateway.Features.Withdrawals.Metrics;
using Common.Telemetry;
using Common.Validation;
using Common.Web.Endpoints;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTelemetry(builder.Host, builder.Environment.ApplicationName);

builder.Services.AddMediatR(cfg => cfg
    .RegisterServicesFromAssembly(Assembly.GetExecutingAssembly())
    .AddOpenBehavior(typeof(ValidationBehavior<,>)));

builder.Services.AddSingleton<RequestMetrics>();
builder.Services.AddSingleton<WithdrawalsMetrics>();

var clientsOptions = builder.Configuration.GetSection(ClientsOptions.SectionName).Get<ClientsOptions>();

builder.Services.AddGrpcClient<BankingService.Client.Withdrawals.WithdrawalsClient>(o =>
{
    o.Address = new Uri(clientsOptions!.BankingService);
});

var app = builder.Build();

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(context =>
    {
        var requestMetrics = context.RequestServices.GetRequiredService<RequestMetrics>();

        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();

        requestMetrics.Exception(exceptionHandlerPathFeature?.Error.GetType().Name ?? "Unknown");

        return Task.CompletedTask;
    });
});

app.MapTelemetry();

app.MapEndpoints();

app.Run();
