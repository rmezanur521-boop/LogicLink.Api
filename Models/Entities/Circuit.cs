using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LogicLink.Api.Models.Entities;

public class Circuit
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public int GridSize { get; set; } = 20;

    public bool SnapToGrid { get; set; } = true;
    public bool ShowGateLabels { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Soft delete — Trash view
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public ICollection<Gate> Gates { get; set; } = new List<Gate>();
    public ICollection<Wire> Wires { get; set; } = new List<Wire>();
}