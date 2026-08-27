using LogicLink.Api.DTOs;
using LogicLink.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LogicLink.Api.Controllers;

[ApiController]
[Route("api/circuits/{circuitId:guid}")]
public class SimulationController : ControllerBase
{
    private readonly ICircuitSimulationService _simulationService;

    public SimulationController(ICircuitSimulationService simulationService)
    {
        _simulationService = simulationService;
    }

    [HttpPost("simulate")]
    public async Task<ActionResult<SimulationResultDto>> Simulate(Guid circuitId, SimulateRequest request)
    {
        try
        {
            var result = await _simulationService.SimulateAsync(circuitId, request.InputOverrides);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpGet("truth-table")]
    public async Task<ActionResult<TruthTableDto>> GetTruthTable(Guid circuitId)
    {
        try
        {
            var result = await _simulationService.GenerateTruthTableAsync(circuitId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}