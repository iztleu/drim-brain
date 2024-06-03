using System.Diagnostics.Metrics;

namespace ApiGateway.Features.Withdrawals.Metrics;

public class WithdrawalsMetrics
{
    private const string MeterName = "ApiGateway";

    private readonly Counter<int> _withdrawalsCreated;

    public WithdrawalsMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);

        _withdrawalsCreated = meter.CreateCounter<int>("drim.city.api_gateway.withdrawals.created");
    }

    public void WithdrawalsCreated(int count)
    {
        _withdrawalsCreated.Add(count);
    }
}
