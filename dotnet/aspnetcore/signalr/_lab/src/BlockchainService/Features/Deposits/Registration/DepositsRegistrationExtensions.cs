namespace BlockchainService.Features.Deposits.Registration;

public static class DepositsRegistrationExtensions
{
    public static WebApplicationBuilder AddDeposits(this WebApplicationBuilder builder)
    {
        return builder;
    }

    public static WebApplication MapDeposits(this WebApplication app)
    {
        app.MapGrpcService<DepositsApi>();

        return app;
    }
}
