using LogicLink.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LogicLink.Api.Controllers;

[ApiController]
[Route("api/circuits/{circuitId:guid}/export")]
public class ExportController : ControllerBase
{
    private readonly IPdfExportService _pdfExportService;

    public ExportController(IPdfExportService pdfExportService)
    {
        _pdfExportService = pdfExportService;
    }

    [HttpGet("pdf")]
    public async Task<IActionResult> ExportPdf(Guid circuitId)
    {
        var pdfBytes = await _pdfExportService.ExportCircuitAsync(circuitId);
        if (pdfBytes is null) return NotFound();

        return File(pdfBytes, "application/pdf", $"circuit-{circuitId}.pdf");
    }
}