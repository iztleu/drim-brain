using BankingService.Features.Deposits.HostedServices;
using BankingService.Features.Deposits.Services;

namespace BankingService.Features.Deposits.Registration;

public static class DepositsRegistrationExtensions
{
    public static WebApplicationBuilder AddDeposits(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<DepositsStream>();
        builder.Services.AddHostedService<DepositConsumerHostedService>();

        return builder;
    }

    public static WebApplication MapDeposits(this WebApplication app)
    {
        app.MapGrpcService<DepositsApi>();

        return app;
    }
}
