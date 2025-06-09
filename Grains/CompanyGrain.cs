using Contracts;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Streams;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Grains;

public sealed class CompanyGrain : Grain, ICompanyGrain
{
    private readonly ILogger<CompanyGrain> _logger;

    private readonly ConcurrentDictionary<string, decimal> _areaResults = new();

    private StreamSubscriptionHandle<OperationUpdateEvent>? _subscription;

    private string CompanyId { get; set; }

    private bool _initialized = false;

    public CompanyGrain(ILogger<CompanyGrain> logger)
    {
        _logger = logger;
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var grainKey = this.GetPrimaryKeyString();
        CompanyId = grainKey.Replace("company:", "");

        var streamProvider = this.GetStreamProvider("Default");
        var streamId = StreamId.Create("OperationalResultUpdate", CompanyId);
        var stream = streamProvider.GetStream<OperationUpdateEvent>(streamId);
        _subscription = await stream.SubscribeAsync(OnAreaUpdate);

        _logger.LogInformation("CompanyGrain: ACTIVATION key {Key}", grainKey);

        await base.OnActivateAsync(cancellationToken);
    }

    public Task InitializeAsync()
    {
        _initialized = true;
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync()
    {
        return Task.FromResult(_initialized);
    }

    private async Task OnAreaUpdate(OperationUpdateEvent evt, StreamSequenceToken? token)
    {
        //var areaResult = evt.HoursWorked > 0 ? (evt.AmountReceived / evt.HoursWorked) : 0m;
        var areaResult = evt.OperationalResult;
        _areaResults[evt.AreaId] = evt.OperationalResult;

        _logger.LogInformation("CompanyGrain: Company {CompanyId} received area {AreaId} update: {Result}", evt.CompanyId, evt.AreaId, areaResult);

        _ = await GetAveragePerformanceAsync();
    }

    public Task<decimal> GetAveragePerformanceAsync()
    {
        if (_areaResults.Count == 0)
            return Task.FromResult(0m);

        decimal sum = 0;
        foreach (var val in _areaResults.Values)
            sum += val;

        var average = sum / _areaResults.Count;

        _logger.LogInformation("CompanyGrain: Company {CompanyId} average performance now is: {Result}", CompanyId, average);

        return Task.FromResult(average);
    }
    public Task ClearPerformanceAsync()
    {
        foreach (var key in _areaResults.Keys)
        {
            _areaResults[key] = 0m;
        }

        _logger.LogInformation("CompanyGrain: Company {CompanyId} performance values cleared. Average is now: 0", CompanyId);

        return Task.CompletedTask;
    }
}
