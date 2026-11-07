using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Churn.Application.DTOs.Imports;
using Churn.Application.Services;

namespace Churn.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class ImportsController : ControllerBase
{
    private readonly ImportService _service;
    public ImportsController(ImportService service) => _service = service;
    [HttpPost("customers/csv")]
    [RequestSizeLimit(50_000_000)]
    public async Task<ActionResult<ImportResultResponse>> ImportCustomers(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0) return BadRequest(new { message = "Choose a non-empty CSV file." });
        if (!Path.GetExtension(file.FileName).Equals(".csv", StringComparison.OrdinalIgnoreCase)) return BadRequest(new { message = "Only CSV files are supported." });
        await using var stream = file.OpenReadStream();
        return Ok(await _service.ImportCsvAsync(stream, ct));
    }
}
