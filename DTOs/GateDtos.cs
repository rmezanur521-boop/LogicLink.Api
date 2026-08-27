using LogicLink.Api.Models.Enums;

namespace LogicLink.Api.DTOs;

public record GateDto(
    Guid Id,
    Guid CircuitId,
    GateType Type,
    double X,
    double Y,
    double Rotation,
    string Label,
    bool? InputValue
);