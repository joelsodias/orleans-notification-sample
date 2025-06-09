using Common.Constants;
using Contracts;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Streams;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Grains;

public sealed class CompanyGrain : Grain<CompanyState>, ICompanyGrain
{
    private readonly ILogger<CompanyGrain> _logger;

    private StreamSubscriptionHandle<OperationUpdateEvent>? _subscription;


    public CompanyGrain(ILogger<CompanyGrain> logger)
    {
        _logger = logger;
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        // Carregar estado persistido
        await ReadStateAsync();
        
        var grainKey = this.GetPrimaryKeyString();
        State.CompanyId = grainKey.Replace("company:", "");

        var streamProvider = this.GetStreamProvider(StreamsConstants.ProviderName);
        var streamId = StreamId.Create(StreamsConstants.OperationalResultUpdateStream, State.CompanyId);
        var stream = streamProvider.GetStream<OperationUpdateEvent>(streamId);
        _subscription = await stream.SubscribeAsync(OnAreaUpdate);


        _logger.LogInformation("CompanyGrain: ACTIVATION key {Key}", grainKey);

        await base.OnActivateAsync(cancellationToken);
    }

    public async Task InitializeAsync()
    {
        State.Initialized = true;
        await WriteStateAsync();
    }

    public Task<bool> ExistsAsync()
    {
        return Task.FromResult(State.Initialized);
    }

    private async Task OnAreaUpdate(OperationUpdateEvent evt, StreamSequenceToken? token)
    {
        State.AreaResults[evt.AreaId] = evt.OperationalResult;

        _logger.LogInformation("CompanyGrain: Company {State.CompanyId} received area {AreaId} update: {Result}", evt.CompanyId, evt.AreaId, evt.OperationalResult);

        await WriteStateAsync();

        // Pode recalcular aqui se quiser usar o valor atualizado imediatamente
        _ = await GetAveragePerformanceAsync();
    }

    public Task<decimal> GetAveragePerformanceAsync()
    {
        if (State.AreaResults.Count == 0)
            return Task.FromResult(0m);

        decimal sum = 0;
        foreach (var val in State.AreaResults.Values)
            sum += val;

        var average = sum / State.AreaResults.Count;

        _logger.LogInformation("CompanyGrain: Company {State.CompanyId} average performance now is: {Result}", State.CompanyId, average);

        return Task.FromResult(average);
    }

    public async Task ClearPerformanceAsync()
    {
        foreach (var key in State.AreaResults.Keys)
        {
            State.AreaResults[key] = 0m;
        }

        await WriteStateAsync();

        _logger.LogInformation("CompanyGrain: Company {State.CompanyId} performance values cleared. Average is now: 0", State.CompanyId);
    }
}
