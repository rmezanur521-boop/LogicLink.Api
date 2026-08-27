using LogicLink.Api.Models.Enums;

namespace LogicLink.Api.Models.Entities;

public class Gate
{
    public Guid Id { get; set; }
    public Guid CircuitId { get; set; }
    public Circuit Circuit { get; set; } = null!;

    public GateType Type { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Rotation { get; set; }
    public string Label { get; set; } = string.Empty;

    public bool? InputValue { get; set; }
}