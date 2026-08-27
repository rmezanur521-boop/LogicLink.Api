namespace LogicLink.Api.DTOs;

public record CircuitSummaryDto(
    Guid Id,
    string Name,
    string OwnerName,
    int GridSize,
    DateTime UpdatedAt
);

public record CircuitDetailDto(
    Guid Id,
    string Name,
    string OwnerName,
    int GridSize,
    bool SnapToGrid,
    bool ShowGateLabels,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<GateDto> Gates,
    IReadOnlyList<WireDto> Wires
);

public record CreateCircuitRequest(
    string Name,
    string OwnerName,
    int? GridSize
);

public record UpdateCircuitSettingsRequest(
    string Name,
    int GridSize,
    bool SnapToGrid,
    bool ShowGateLabels
);