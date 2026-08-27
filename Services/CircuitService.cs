using LogicLink.Api.Data;
using LogicLink.Api.DTOs;
using LogicLink.Api.Models.Entities;
using LogicLink.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LogicLink.Api.Services;

public class CircuitService : ICircuitService
{
    private readonly AppDbContext _db;

    public CircuitService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<CircuitSummaryDto>> GetAllAsync()
    {
        return await _db.Circuits
            .Where(c => !c.IsDeleted)
            .OrderByDescending(c => c.UpdatedAt)
            .Select(c => ToSummaryDto(c))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<CircuitSummaryDto>> GetTrashAsync()
    {
        return await _db.Circuits
            .Where(c => c.IsDeleted)
            .OrderByDescending(c => c.DeletedAt)
            .Select(c => ToSummaryDto(c))
            .ToListAsync();
    }

    public async Task<CircuitDetailDto?> GetByIdAsync(Guid id)
    {
        var circuit = await _db.Circuits
            .Include(c => c.Gates)
            .Include(c => c.Wires)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        return circuit is null ? null : ToDetailDto(circuit);
    }

    public async Task<CircuitSummaryDto> CreateAsync(CreateCircuitRequest request)
    {
        var circuit = new Circuit
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            OwnerName = request.OwnerName.Trim(),
            GridSize = request.GridSize ?? 20
        };

        _db.Circuits.Add(circuit);
        await _db.SaveChangesAsync();

        return ToSummaryDto(circuit);
    }

    public async Task<CircuitDetailDto?> UpdateSettingsAsync(Guid id, UpdateCircuitSettingsRequest request)
    {
        var circuit = await _db.Circuits
            .Include(c => c.Gates)
            .Include(c => c.Wires)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        if (circuit is null) return null;

        circuit.Name = request.Name.Trim();
        circuit.GridSize = request.GridSize;
        circuit.SnapToGrid = request.SnapToGrid;
        circuit.ShowGateLabels = request.ShowGateLabels;

        await _db.SaveChangesAsync();
        return ToDetailDto(circuit);
    }

    public async Task<bool> SoftDeleteAsync(Guid id)
    {
        var circuit = await _db.Circuits.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        if (circuit is null) return false;

        circuit.IsDeleted = true;
        circuit.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RestoreAsync(Guid id)
    {
        var circuit = await _db.Circuits.FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted);
        if (circuit is null) return false;

        circuit.IsDeleted = false;
        circuit.DeletedAt = null;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> PermanentDeleteAsync(Guid id)
    {
        var circuit = await _db.Circuits.FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted);
        if (circuit is null) return false;

        _db.Circuits.Remove(circuit);
        await _db.SaveChangesAsync();
        return true;
    }

    private static CircuitSummaryDto ToSummaryDto(Circuit c) =>
        new(c.Id, c.Name, c.OwnerName, c.GridSize, c.UpdatedAt);

    private static CircuitDetailDto ToDetailDto(Circuit c) => new(
        c.Id, c.Name, c.OwnerName, c.GridSize, c.SnapToGrid, c.ShowGateLabels,
        c.CreatedAt, c.UpdatedAt,
        c.Gates.Select(g => new GateDto(g.Id, g.CircuitId, g.Type, g.X, g.Y, g.Rotation, g.Label, g.InputValue)).ToList(),
        c.Wires.Select(w => new WireDto(w.Id, w.CircuitId, w.FromGateId, w.FromPinIndex, w.ToGateId, w.ToPinIndex)).ToList()
    );
}