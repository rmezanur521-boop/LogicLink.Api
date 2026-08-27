namespace LogicLink.Api.Services.Interfaces;

public interface IPdfExportService
{
    Task<byte[]?> ExportCircuitAsync(Guid circuitId);
}