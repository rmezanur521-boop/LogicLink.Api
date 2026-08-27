using LogicLink.Api.DTOs;

namespace LogicLink.Api.Services.Interfaces;

public interface ICircuitService
{
    Task<IReadOnlyList<CircuitSummaryDto>> GetAllAsync();
    Task<IReadOnlyList<CircuitSummaryDto>> GetTrashAsync();
    Task<CircuitDetailDto?> GetByIdAsync(Guid id);
    Task<CircuitSummaryDto> CreateAsync(CreateCircuitRequest request);
    Task<CircuitDetailDto?> UpdateSettingsAsync(Guid id, UpdateCircuitSettingsRequest request);
    Task<bool> SoftDeleteAsync(Guid id);
    Task<bool> RestoreAsync(Guid id);
    Task<bool> PermanentDeleteAsync(Guid id);
    Task<GateDto> AddGateAsync(Guid circuitId, GateDto gate);
    Task<GateDto?> MoveGateAsync(Guid circuitId, Guid gateId, double x, double y, double rotation);
    Task<bool> DeleteGateAsync(Guid circuitId, Guid gateId);
    Task<WireDto> AddWireAsync(Guid circuitId, WireDto wire);
    Task<bool> DeleteWireAsync(Guid circuitId, Guid wireId);
}