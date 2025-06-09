
using System.Collections.Concurrent;

namespace Contracts;

public class AreaState
{
    public decimal HoursWorked { get; set; }
    public decimal AmountReceived { get; set; }
    public decimal Factor { get; set; } = 1m;
    public decimal CurrentOperationalResult { get; set; }
}


public class CompanyState
{
    public ConcurrentDictionary<string, decimal> AreaResults { get; set; } = new();
    public bool Initialized { get; set; } = false;
}