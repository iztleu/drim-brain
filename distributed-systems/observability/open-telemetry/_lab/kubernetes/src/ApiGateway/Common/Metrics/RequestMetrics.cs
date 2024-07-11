using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace ApiGateway.Common.Metrics;

public class RequestMetrics
{
    private const string MeterName = "ApiGateway";

    private readonly Counter<int> _requests;
    private readonly Counter<int> _requestsSucceeded;
    private readonly Counter<int> _requestsFailed;
    private readonly UpDownCounter<int> _requestsInProgress;
    private readonly Histogram<double> _requestDuration;
    private readonly Counter<int> _exceptions;

    public RequestMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);

        _requests = meter.CreateCounter<int>(
            "city.drim.api_gateway.requests",
            description: "Total number of requests");

        _requestsSucceeded = meter.CreateCounter<int>(
            "city.drim.api_gateway.requests.succeeded",
            description: "Total number of requests succeeded");

        _requestsFailed = meter.CreateCounter<int>(
            "city.drim.api_gateway.requests.failed",
            description: "Total number of requests failed");

        _requestsInProgress = meter.CreateUpDownCounter<int>(
            "city.drim.api_gateway.requests.in_progress",
            description: "Number of requests in progress");

        _requestDuration = meter.CreateHistogram<double>(
            "city.drim.api_gateway.request_duration",
            unit: "ms",
            description: "Request duration in milliseconds");

        _exceptions = meter.CreateCounter<int>(
            "city.drim.api_gateway.exceptions",
            description: "Total number of exceptions");
    }

    public async Task<T> Measure<T>(string name, Func<Task<T>> request)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            _requests.Add(1, new KeyValuePair<string, object?>("request", name));
            _requestsInProgress.Add(1, new KeyValuePair<string, object?>("request", name));
            var response = await request();
            _requestsSucceeded.Add(1, new KeyValuePair<string, object?>("request", name));
            return response;
        }
        catch
        {
            _requestsFailed.Add(1, new KeyValuePair<string, object?>("request", name));
            throw;
        }
        finally
        {
            sw.Stop();
            _requestDuration.Record(sw.ElapsedMilliseconds, new KeyValuePair<string, object?>("request", name));
            _requestsInProgress.Add(-1, new KeyValuePair<string, object?>("request", name));
        }
    }

    public void Exception(string type)
    {
        _exceptions.Add(1, new KeyValuePair<string, object?>("type", type));
    }
}
