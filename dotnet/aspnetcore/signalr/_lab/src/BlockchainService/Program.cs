using System.Reflection;
using BlockchainService.Features.Deposits.Registration;
using BlockchainService.Features.Withdrawals.Registration;
using Common.Validation;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(Path.Combine("extraSettings", "appsettings.json"), optional: false, reloadOnChange: true);

builder.Services.AddMediatR(cfg => cfg
    .RegisterServicesFromAssembly(Assembly.GetExecutingAssembly())
    .AddOpenBehavior(typeof(ValidationBehavior<,>)));

builder.Services.AddGrpc();

builder.AddDeposits();
builder.AddWithdrawals();

var app = builder.Build();

app.MapDeposits();
app.MapWithdrawals();

app.Run();
