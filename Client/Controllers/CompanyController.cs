using Microsoft.AspNetCore.Mvc;
using Orleans;
using Contracts;
using System.Threading.Tasks;

namespace CompanyPerformance.Api.Controllers;

[ApiController]
[Route("api/company")]
public class CompanyController : ControllerBase
{
    private readonly IClusterClient _client;

    public CompanyController(IClusterClient client)
    {
        _client = client;
    }

    [HttpGet("operational/result")]
    public async Task<IActionResult> GetOperationalResult([FromQuery] string companyId)
    {
        var companyGrain = _client.GetGrain<ICompanyGrain>(companyId);
        var average = await companyGrain.GetAveragePerformanceAsync();
        return Ok(new { company = companyId, average });
    }
}
