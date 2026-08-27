using LogicLink.Api.DTOs;
using LogicLink.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LogicLink.Api.Controllers;

[ApiController]
[Route("api/circuits")]
public class CircuitsController : ControllerBase
{
    private readonly ICircuitService _circuitService;

    public CircuitsController(ICircuitService circuitService)
    {
        _circuitService = circuitService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CircuitSummaryDto>>> GetAll()
        => Ok(await _circuitService.GetAllAsync());

    [HttpGet("trash")]
    public async Task<ActionResult<IReadOnlyList<CircuitSummaryDto>>> GetTrash()
        => Ok(await _circuitService.GetTrashAsync());

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CircuitDetailDto>> GetById(Guid id)
    {
        var circuit = await _circuitService.GetByIdAsync(id);
        return circuit is null ? NotFound() : Ok(circuit);
    }

    [HttpPost]
    public async Task<ActionResult<CircuitSummaryDto>> Create(CreateCircuitRequest request)
    {
        var created = await _circuitService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}/settings")]
    public async Task<ActionResult<CircuitDetailDto>> UpdateSettings(Guid id, UpdateCircuitSettingsRequest request)
    {
        var updated = await _circuitService.UpdateSettingsAsync(id, request);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> SoftDelete(Guid id)
        => await _circuitService.SoftDeleteAsync(id) ? NoContent() : NotFound();

    [HttpPost("{id:guid}/restore")]
    public async Task<IActionResult> Restore(Guid id)
        => await _circuitService.RestoreAsync(id) ? NoContent() : NotFound();

    [HttpDelete("{id:guid}/permanent")]
    public async Task<IActionResult> PermanentDelete(Guid id)
        => await _circuitService.PermanentDeleteAsync(id) ? NoContent() : NotFound();
}