namespace LogicLink.Api.DTOs;

public record WireDto(
    Guid Id,
    Guid CircuitId,
    Guid FromGateId,
    int FromPinIndex,
    Guid ToGateId,
    int ToPinIndex
);