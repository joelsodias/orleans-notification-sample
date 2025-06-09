using Contracts;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Streams;
using System.Threading;
using System.Threading.Tasks;

namespace Grains;

public sealed class AreaGrain : Grain, IAreaGrain
{
    private readonly ILogger<AreaGrain> _logger;

    private decimal _hoursWorked;
    private decimal _amountReceived;
    private decimal _factor = 1m;
    private decimal _currentOperationalResult = 0;



    private string _companyId = string.Empty;
    private string _areaId = string.Empty;

    private IAsyncStream<OperationUpdateEvent>? _companyStream;
    private StreamSubscriptionHandle<FactorChangedEvent>? _factorSubscription;

    public AreaGrain(ILogger<AreaGrain> logger)
    {
        _logger = logger;
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var grainKey = this.GetPrimaryKeyString();

        // Esperado: "company:{CompanyId}_area:{AreaId}"
        var parts = grainKey.Split("_area:");
        _companyId = parts[0].Replace("company:", "");
        _areaId = parts[1];

        var streamProvider = this.GetStreamProvider("Default");

        var streamId = StreamId.Create("OperationalResultUpdate", _companyId);
        _companyStream = streamProvider.GetStream<OperationUpdateEvent>(streamId);

        var factorStreamId = StreamId.Create("CompanyFactorUpdates", _companyId);
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

    private decimal CalcResultInternal()
    {
        var oldOperationalResult = _currentOperationalResult;
        _currentOperationalResult = (_amountReceived / _hoursWorked) * _factor;
        _logger.LogInformation("AreaGrain: Area {AreaId} performance changed from {Old} to {New}", _areaId, oldOperationalResult, _currentOperationalResult);
        if (_hoursWorked <= 0) return 0;
        return _currentOperationalResult;
    }

    public Task<decimal> GetCurrentResultAsync()
    {
        return Task.FromResult(_currentOperationalResult);
    }

    private async Task OnFactorChangedAsync(FactorChangedEvent evt, StreamSequenceToken? token)
    {
        _factor = evt.NewFactor;
        _logger.LogInformation("AreaGrain: Area {AreaId} received new factor {Factor} from event", _areaId, _factor);

        await RecalculateAndNotifyAsync();
    }

    public async Task UpdateFactorAsync(decimal newFactor)
    {
        _logger.LogInformation("AreaGrain: Area {AreaId} factor manually updated from {OldFactor} to {Factor}", _areaId, _factor, newFactor);
        _factor = newFactor;
        await RecalculateAndNotifyAsync();
    }

    private async Task RecalculateAndNotifyAsync()
    {
        _logger.LogInformation("AreaGrain: Area {AreaId} recalculating performance", _areaId);
        CalcResultInternal();
        await NotifyCompany();
    }

    public async Task UpdateOperationAsync(decimal hoursWorked, decimal amountReceived)
    {
        _logger.LogInformation("AreaGrain: Area {AreaId} parameters manually changed \n - Hours Worked from {FromHours} to {ToHours} \n - Amount Received: {FromAmount} to {ToAmount}",
            _areaId,
            _hoursWorked,
            hoursWorked,
            _amountReceived,
            amountReceived
        );

        _amountReceived = amountReceived;
        _hoursWorked = hoursWorked;

        await RecalculateAndNotifyAsync();
    }

    private Task NotifyCompany()
    {
        var evt = new OperationUpdateEvent(
            CompanyId: _companyId,
            AreaId: _areaId,
            HoursWorked: _hoursWorked,
            AmountReceived: _amountReceived,
            Factor: _factor,
            OperationalResult: _currentOperationalResult
        );

        _logger.LogInformation("AreaGrain: Notifying company {Company} about changes in area {Area} about current operational result: {Result}", _companyId, _areaId, _currentOperationalResult);
        return _companyStream?.OnNextAsync(evt) ?? Task.CompletedTask;
    }




}
