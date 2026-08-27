namespace LogicLink.Api.DTOs;

public record SimulateRequest(Dictionary<Guid, bool> InputOverrides);

public record GateValueDto(Guid GateId, string Label, bool Value);

public record SimulationResultDto(
    IReadOnlyDictionary<Guid, bool> GateValues,
    IReadOnlyList<GateValueDto> Outputs
);

public record TruthTableDto(
    IReadOnlyList<string> InputLabels,
    IReadOnlyList<string> OutputLabels,
    IReadOnlyList<TruthTableRowDto> Rows
);

public record TruthTableRowDto(IReadOnlyList<bool> Inputs, IReadOnlyList<bool> Outputs);