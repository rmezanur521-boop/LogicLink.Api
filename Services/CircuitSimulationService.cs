using LogicLink.Api.Data;
using LogicLink.Api.DTOs;
using LogicLink.Api.Models.Entities;
using LogicLink.Api.Models.Enums;
using LogicLink.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LogicLink.Api.Services;

public class CircuitSimulationService : ICircuitSimulationService
{
    private const int MaxTruthTableInputs = 10; // 2^10 = 1024 rows, তার বেশি হলে UI/response দুটোই আনরিডেবল হয়ে যায়

    private readonly AppDbContext _db;

    public CircuitSimulationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<SimulationResultDto> SimulateAsync(Guid circuitId, Dictionary<Guid, bool>? inputOverrides)
    {
        var (gates, wires) = await LoadGraphAsync(circuitId);
        var evaluator = new GraphEvaluator(gates, wires, inputOverrides ?? new Dictionary<Guid, bool>());

        var values = evaluator.EvaluateAll();
        var outputs = gates
            .Where(g => g.Type == GateType.Output)
            .Select(g => new GateValueDto(g.Id, g.Label, values.GetValueOrDefault(g.Id)))
            .ToList();

        return new SimulationResultDto(values, outputs);
    }

    public async Task<TruthTableDto> GenerateTruthTableAsync(Guid circuitId)
    {
        var (gates, wires) = await LoadGraphAsync(circuitId);

        var inputGates = gates.Where(g => g.Type == GateType.Input).OrderBy(g => g.X).ToList();
        var outputGates = gates.Where(g => g.Type == GateType.Output).OrderBy(g => g.X).ToList();

        if (inputGates.Count > MaxTruthTableInputs)
            throw new InvalidOperationException($"Truth table supports at most {MaxTruthTableInputs} inputs, this circuit has {inputGates.Count}.");

        var rows = new List<TruthTableRowDto>();
        var combinationCount = 1 << inputGates.Count; // 2^n

        for (var combo = 0; combo < combinationCount; combo++)
        {
            var overrides = new Dictionary<Guid, bool>();
            var inputValues = new List<bool>();

            for (var bit = 0; bit < inputGates.Count; bit++)
            {
                var value = ((combo >> (inputGates.Count - 1 - bit)) & 1) == 1;
                overrides[inputGates[bit].Id] = value;
                inputValues.Add(value);
            }

            var evaluator = new GraphEvaluator(gates, wires, overrides);
            var values = evaluator.EvaluateAll();
            var outputValues = outputGates.Select(g => values.GetValueOrDefault(g.Id)).ToList();

            rows.Add(new TruthTableRowDto(inputValues, outputValues));
        }

        return new TruthTableDto(
            inputGates.Select(g => g.Label).ToList(),
            outputGates.Select(g => g.Label).ToList(),
            rows
        );
    }

    private async Task<(List<Gate> Gates, List<Wire> Wires)> LoadGraphAsync(Guid circuitId)
    {
        var gates = await _db.Gates.Where(g => g.CircuitId == circuitId).ToListAsync();
        var wires = await _db.Wires.Where(w => w.CircuitId == circuitId).ToListAsync();
        return (gates, wires);
    }

    private sealed class GraphEvaluator
    {
        private readonly Dictionary<Guid, Gate> _gatesById;
        private readonly ILookup<Guid, Wire> _incomingWiresByTargetGate;
        private readonly Dictionary<Guid, bool> _overrides;
        private readonly Dictionary<Guid, bool> _memo = new();
        private readonly HashSet<Guid> _visiting = new();

        public GraphEvaluator(List<Gate> gates, List<Wire> wires, Dictionary<Guid, bool> overrides)
        {
            _gatesById = gates.ToDictionary(g => g.Id);
            _incomingWiresByTargetGate = wires.ToLookup(w => w.ToGateId);
            _overrides = overrides;
        }

        public Dictionary<Guid, bool> EvaluateAll()
        {
            foreach (var gateId in _gatesById.Keys)
                Evaluate(gateId);

            return _memo;
        }

        private bool Evaluate(Guid gateId)
        {
            if (_memo.TryGetValue(gateId, out var cached)) return cached;

            if (!_visiting.Add(gateId))
                throw new InvalidOperationException("Circuit contains a feedback loop — combinational simulation is not supported.");

            var gate = _gatesById[gateId];
            var result = gate.Type == GateType.Input
                ? _overrides.GetValueOrDefault(gateId, gate.InputValue ?? false)
                : ComputeFromInputs(gate);

            _visiting.Remove(gateId);
            _memo[gateId] = result;
            return result;
        }

        private bool ComputeFromInputs(Gate gate)
        {
            var incomingValues = _incomingWiresByTargetGate[gate.Id]
                .OrderBy(w => w.ToPinIndex)
                .Select(w => Evaluate(w.FromGateId))
                .ToList();

            return gate.Type switch
            {
                GateType.Output => incomingValues.FirstOrDefault(),
                GateType.Not => !incomingValues.FirstOrDefault(),
                GateType.And => incomingValues.Count > 0 && incomingValues.All(v => v),
                GateType.Or => incomingValues.Any(v => v),
                GateType.Nand => !(incomingValues.Count > 0 && incomingValues.All(v => v)),
                GateType.Nor => !incomingValues.Any(v => v),
                GateType.Xor => incomingValues.Count(v => v) % 2 == 1,
                _ => throw new InvalidOperationException($"Unsupported gate type: {gate.Type}")
            };
        }
    }
}