namespace LogicLink.Api.Models.Entities;

public class Wire
{
    public Guid Id { get; set; }
    public Guid CircuitId { get; set; }
    public Circuit Circuit { get; set; } = null!;

    public Guid FromGateId { get; set; }
    public int FromPinIndex { get; set; }

    public Guid ToGateId { get; set; }
    public int ToPinIndex { get; set; }
}