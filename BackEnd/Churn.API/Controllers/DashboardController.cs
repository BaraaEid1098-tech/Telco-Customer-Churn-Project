using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Churn.Application.DTOs.Dashboard;
using Churn.Application.Services;

namespace Churn.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class DashboardController : ControllerBase
{
    private readonly DashboardService _service;
    public DashboardController(DashboardService service) => _service = service;
    [HttpGet("summary")] public async Task<ActionResult<DashboardSummaryResponse>> GetSummary(CancellationToken ct) => Ok(await _service.GetSummaryAsync(ct));
}
