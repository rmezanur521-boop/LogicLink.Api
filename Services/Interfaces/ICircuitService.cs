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
}