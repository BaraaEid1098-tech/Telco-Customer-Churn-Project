using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Churn.Application.DTOs.Campaigns;
using Churn.Application.Services;

namespace Churn.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class CampaignsController : ControllerBase
{
    private readonly CampaignService _service;
    public CampaignsController(CampaignService service) => _service = service;
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CampaignResponse>>> GetAll(CancellationToken ct) => Ok(await _service.GetAllAsync(ct));
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CampaignResponse>> GetById(int id, CancellationToken ct)
    { var item = await _service.GetByIdAsync(id, ct); return item is null ? NotFound() : Ok(item); }
    [HttpPost] 
    public async Task<ActionResult<CampaignResponse>> Create(CreateCampaignRequest request, CancellationToken ct)
    { var item = await _service.CreateAsync(request, ct); return CreatedAtAction(nameof(GetById), new { id = item.Id }, item); }
    [HttpPut("{id:int}")]
    public async Task<ActionResult<CampaignResponse>> Update(int id, UpdateCampaignRequest request, CancellationToken ct)
    { var item = await _service.UpdateAsync(id, request, ct); return item is null ? NotFound() : Ok(item); }
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct) =>
        await _service.DeleteAsync(id, ct) ? NoContent() : NotFound();
    [HttpGet("{id:int}/customers")]
    public async Task<IActionResult> GetAudience(int id, CancellationToken ct)
    { var items = await _service.GetAudienceAsync(id, ct); return items is null ? NotFound() : Ok(items); }
    [HttpPost("{id:int}/customers")]
    public async Task<IActionResult> AddCustomer(int id, AddCampaignCustomerRequest request, CancellationToken ct)
    { var item = await _service.AddCustomerAsync(id, request, ct); return item is null ? NotFound() : CreatedAtAction(nameof(GetAudience), new { id }, item); }
    [HttpPut("{id:int}/customers/{memberId:int}")] 
    public async Task<IActionResult> UpdateCustomer(int id, int memberId, UpdateCampaignCustomerRequest request, CancellationToken ct)
    { var item = await _service.UpdateCustomerAsync(id, memberId, request, ct); return item is null ? NotFound() : Ok(item); }
}
