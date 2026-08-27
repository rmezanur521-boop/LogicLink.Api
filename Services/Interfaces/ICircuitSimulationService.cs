using LogicLink.Api.DTOs;

namespace LogicLink.Api.Services.Interfaces;

public interface ICircuitSimulationService
{
    Task<SimulationResultDto> SimulateAsync(Guid circuitId, Dictionary<Guid, bool>? inputOverrides);
    Task<TruthTableDto> GenerateTruthTableAsync(Guid circuitId);
}