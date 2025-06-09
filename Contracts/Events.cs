using Orleans;
using Orleans.Serialization;
using System;

namespace Contracts;

[GenerateSerializer]
public record AreaKey(int CompanyId, int AreaId);

[GenerateSerializer]
public sealed record OperationUpdateEvent(
    [property: Id(0)] string CompanyId,
    [property: Id(1)] string AreaId,
    [property: Id(2)] decimal HoursWorked,
    [property: Id(3)] decimal AmountReceived,
    [property: Id(4)] decimal Factor,
    [property: Id(5)] decimal OperationalResult

);

[GenerateSerializer]
public sealed record FactorChangedEvent(
    [property: Id(0)] string CompanyId,
    [property: Id(1)] decimal NewFactor
);
