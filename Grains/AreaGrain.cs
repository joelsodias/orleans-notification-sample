using Contracts;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Streams;
using System.Threading;
using System.Threading.Tasks;

namespace Grains;

public sealed class AreaGrain : Grain<AreaState>, IAreaGrain
{
    private readonly ILogger<AreaGrain> _logger;

    private IAsyncStream<OperationUpdateEvent>? _companyStream;
    private StreamSubscriptionHandle<FactorChangedEvent>? _factorSubscription;

    public AreaGrain(ILogger<AreaGrain> logger)
    {
        _logger = logger;
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var grainKey = this.GetPrimaryKeyString();

        await ReadStateAsync();
        
        // Esperado: "company:{CompanyId}_area:{AreaId}"
        var parts = grainKey.Split("_area:");
        State.CompanyId = parts[0].Replace("company:", "");
        State.AreaId = parts[1];


        var streamProvider = this.GetStreamProvider("Default");

        var streamId = StreamId.Create("OperationalResultUpdate", State.CompanyId);
        _companyStream = streamProvider.GetStream<OperationUpdateEvent>(streamId);

        var factorStreamId = StreamId.Create("CompanyFactorUpdates", State.CompanyId);
        var factorStream = streamProvider.GetStream<FactorChangedEvent>(factorStreamId);
        _factorSubscription = await factorStream.SubscribeAsync(OnFactorChangedAsync);

        _logger.LogInformation("AreaGrain: ACTIVATION key {Key}", grainKey);

        await base.OnActivateAsync(cancellationToken);
    }

    public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        if (_factorSubscription is not null)
            await _factorSubscription.UnsubscribeAsync();
    }

    private async Task<decimal> CalcResultInternalAsync()
    {
        var oldOperationalResult = State.CurrentOperationalResult;
        State.CurrentOperationalResult = State.AmountReceived / State.HoursWorked * State.Factor;
        await WriteStateAsync();
        _logger.LogInformation("AreaGrain: Area {AreaId} performance changed from {Old} to {New}", State.AreaId, oldOperationalResult, State.CurrentOperationalResult);
        if (State.HoursWorked <= 0) return 0;
        return State.CurrentOperationalResult;
    }

    public Task<decimal> GetCurrentResultAsync()
    {
        return Task.FromResult(State.CurrentOperationalResult);
    }

    private async Task OnFactorChangedAsync(FactorChangedEvent evt, StreamSequenceToken? token)
    {
        State.Factor = evt.NewFactor;
        await WriteStateAsync();
        _logger.LogInformation("AreaGrain: Area {AreaId} received new factor {Factor} from event", State.AreaId, State.Factor);

        await RecalculateAndNotifyAsync();
    }

    public async Task UpdateFactorAsync(decimal newFactor)
    {
        _logger.LogInformation("AreaGrain: Area {AreaId} factor manually updated from {OldFactor} to {Factor}", State.AreaId, State.Factor, newFactor);
        State.Factor = newFactor;
        await WriteStateAsync();
        await RecalculateAndNotifyAsync();
    }

    private async Task RecalculateAndNotifyAsync()
    {
        _logger.LogInformation("AreaGrain: Area {AreaId} recalculating performance", State.AreaId);
        await CalcResultInternalAsync();
        await WriteStateAsync();
        await NotifyCompany();
    }

    public async Task UpdateOperationAsync(decimal hoursWorked, decimal amountReceived)
    {
        _logger.LogInformation("AreaGrain: Area {AreaId} parameters manually changed \n - Hours Worked from {FromHours} to {ToHours} \n - Amount Received: {FromAmount} to {ToAmount}",
            State.AreaId,
            State.HoursWorked,
            hoursWorked,
            State.AmountReceived,
            amountReceived
        );

        State.AmountReceived = amountReceived;
        State.HoursWorked = hoursWorked;
        await WriteStateAsync();
        await RecalculateAndNotifyAsync();
    }

    private async Task NotifyCompany()
    {

        var companyGrain = GrainFactory.GetGrain<ICompanyGrain>(State.CompanyId);

        var exists = await companyGrain.ExistsAsync();
        if (!exists)
        {
            await companyGrain.InitializeAsync();
        }

        var evt = new OperationUpdateEvent(
            CompanyId: State.CompanyId,
            AreaId: State.AreaId,
            HoursWorked: State.HoursWorked,
            AmountReceived: State.AmountReceived,
            Factor: State.Factor,
            OperationalResult: State.CurrentOperationalResult
        );

        _logger.LogInformation("AreaGrain: Notifying company {Company} about changes in area {Area} about current operational result: {Result}", State.CompanyId, State.AreaId, State.CurrentOperationalResult);

        await _companyStream?.OnNextAsync(evt);
    }




}
