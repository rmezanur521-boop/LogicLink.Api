using LogicLink.Api.Data;
using LogicLink.Api.DTOs;
using LogicLink.Api.Models.Entities;
using LogicLink.Api.Models.Enums;
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
    public async Task<GateDto> AddGateAsync(Guid circuitId, GateDto gate)
    {
        var entity = new Gate
        {
            Id = Guid.NewGuid(),
            CircuitId = circuitId,
            Type = gate.Type,
            X = gate.X,
            Y = gate.Y,
            Rotation = gate.Rotation,
            Label = gate.Label,
            InputValue = gate.Type == GateType.Input ? gate.InputValue ?? false : null
        };

        _db.Gates.Add(entity);
        await _db.SaveChangesAsync();

        return new GateDto(entity.Id, entity.CircuitId, entity.Type, entity.X, entity.Y, entity.Rotation, entity.Label, entity.InputValue);
    }

    public async Task<GateDto?> MoveGateAsync(Guid circuitId, Guid gateId, double x, double y, double rotation)
    {
        var gate = await _db.Gates.FirstOrDefaultAsync(g => g.Id == gateId && g.CircuitId == circuitId);
        if (gate is null) return null;

        gate.X = x;
        gate.Y = y;
        gate.Rotation = rotation;
        await _db.SaveChangesAsync();

        return new GateDto(gate.Id, gate.CircuitId, gate.Type, gate.X, gate.Y, gate.Rotation, gate.Label, gate.InputValue);
    }

    public async Task<bool> DeleteGateAsync(Guid circuitId, Guid gateId)
    {
        var gate = await _db.Gates.FirstOrDefaultAsync(g => g.Id == gateId && g.CircuitId == circuitId);
        if (gate is null) return false;

         var connectedWires = _db.Wires.Where(w => w.FromGateId == gateId || w.ToGateId == gateId);
        _db.Wires.RemoveRange(connectedWires);
        _db.Gates.Remove(gate);

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<WireDto> AddWireAsync(Guid circuitId, WireDto wire)
    {
        var entity = new Wire
        {
            Id = Guid.NewGuid(),
            CircuitId = circuitId,
            FromGateId = wire.FromGateId,
            FromPinIndex = wire.FromPinIndex,
            ToGateId = wire.ToGateId,
            ToPinIndex = wire.ToPinIndex
        };

        _db.Wires.Add(entity);
        await _db.SaveChangesAsync();

        return new WireDto(entity.Id, entity.CircuitId, entity.FromGateId, entity.FromPinIndex, entity.ToGateId, entity.ToPinIndex);
    }

    public async Task<bool> DeleteWireAsync(Guid circuitId, Guid wireId)
    {
        var wire = await _db.Wires.FirstOrDefaultAsync(w => w.Id == wireId && w.CircuitId == circuitId);
        if (wire is null) return false;

        _db.Wires.Remove(wire);
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