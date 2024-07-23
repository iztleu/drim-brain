using BankingService.Database;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<BankingDbContext>("BankingServiceDb");

var app = builder.Build();

app.MapDefaultEndpoints();

app.Run();
