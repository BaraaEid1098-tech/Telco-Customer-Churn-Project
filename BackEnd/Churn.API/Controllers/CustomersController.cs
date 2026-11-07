using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Churn.Application.DTOs.Customers;
using Churn.Application.Services;

namespace Churn.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class CustomersController : ControllerBase
{
    private readonly CustomerService _service;
    public CustomersController(CustomerService service) => _service = service;

    [HttpGet] 
    public async Task<ActionResult<IReadOnlyList<CustomerResponse>>> GetAll(CancellationToken ct) => Ok(await _service.GetAllAsync(ct));
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CustomerResponse>> GetById(int id, CancellationToken ct)
    { var item = await _service.GetByIdAsync(id, ct); return item is null ? NotFound() : Ok(item); }
    [HttpPost]
    public async Task<ActionResult<CustomerResponse>> Create(CreateCustomerRequest request, CancellationToken ct)
    { var item = await _service.CreateAsync(request, ct); return CreatedAtAction(nameof(GetById), new { id = item.Id }, item); }
    [HttpPut("{id:int}")]
    public async Task<ActionResult<CustomerResponse>> Update(int id, UpdateCustomerRequest request, CancellationToken ct)
    { var item = await _service.UpdateAsync(id, request, ct); return item is null ? NotFound() : Ok(item); }
    [HttpDelete("{id:int}")] 
    public async Task<IActionResult> Delete(int id, CancellationToken ct) =>
        await _service.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
