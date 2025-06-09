using Orleans;
using System.Threading.Tasks;

namespace Contracts;

public interface IAreaGrain : IGrainWithStringKey
{
    Task UpdateOperationAsync(decimal hoursWorked, decimal amountReceived);
    Task UpdateFactorAsync(decimal newFactor);
    Task<decimal> GetCurrentResultAsync(); // <- Deve ser exatamente assim
}

public interface ICompanyGrain : IGrainWithStringKey
{
    Task InitializeAsync();
    Task<bool> ExistsAsync();
    Task<decimal> GetAveragePerformanceAsync();
    Task ClearPerformanceAsync();

}
