using Microsoft.AspNetCore.Mvc;
using Orleans;
using Orleans.Streams;
using Contracts;
using System.Threading.Tasks;
using Common.Constants;

namespace CompanyPerformance.Api.Controllers;

[ApiController]
[Route("api/company")]
public class AreaController : ControllerBase
{
    private readonly IClusterClient _client;

    public AreaController(IClusterClient client)
    {
        _client = client;
    }

    [HttpPost("/{companyId}/area/{areaId}/update")]
    public async Task<IActionResult> UpdateOperation(
        string companyId,
        string areaId,
        [FromQuery] decimal hours,
        [FromQuery] decimal amount)
    {
        var areaGrain = _client.GetGrain<IAreaGrain>($"company:{companyId}_area:{areaId}");
        await areaGrain.UpdateOperationAsync(hours, amount);
        return Ok(new { area = areaId, hours, amount });
    }

    [HttpPost("/{companyId}/factor")]
    public async Task<IActionResult> UpdateFactor(
        string companyId,
        [FromQuery] decimal factor)
    {
        var streamProvider = _client.GetStreamProvider(StreamsConstants.ProviderName);
        var factorStream = streamProvider.GetStream<FactorChangedEvent>(
            StreamId.Create(StreamsConstants.FactorUpdateStream, companyId)
        );

        await factorStream.OnNextAsync(new FactorChangedEvent(companyId, factor));
        return Ok(new { company = companyId, factor });
    }
}
