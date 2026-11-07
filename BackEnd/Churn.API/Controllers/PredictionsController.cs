using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Churn.Application.DTOs.Predictions;
using Churn.Application.Services;

namespace Churn.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class PredictionsController : ControllerBase
{
    private readonly PredictionService _service;
    public PredictionsController(PredictionService service) => _service = service;
    [HttpGet] 
    public async Task<ActionResult<IReadOnlyList<PredictionResponse>>> GetAll(CancellationToken ct) => Ok(await _service.GetAllAsync(ct));
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PredictionResponse>> GetById(int id, CancellationToken ct)
    { var item = await _service.GetByIdAsync(id, ct); return item is null ? NotFound() : Ok(item); }
    [HttpPost("{customerId:int}")]
    public async Task<ActionResult<PredictionResponse>> Predict(int customerId, CancellationToken ct)
    { 
        var item = await _service.PredictAsync(customerId, ct); return item is null ? NotFound(new { message = "Customer not found." }) : Ok(item); 
    }
}
