using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Churn.API.Controllers;

[ApiController]
[Route("api/ml")]
[Authorize]
public sealed class MLStatusController(IHttpClientFactory httpClientFactory) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("health")]
    public Task<IActionResult> GetHealth(CancellationToken cancellationToken) =>
        ProxyAsync("health", cancellationToken);

    [HttpGet("models")]
    public Task<IActionResult> GetModels(CancellationToken cancellationToken) =>
        ProxyAsync("models", cancellationToken);

    private async Task<IActionResult> ProxyAsync(
        string path,
        CancellationToken cancellationToken)
    {
        using var response = await httpClientFactory
            .CreateClient("MLServiceGateway")
            .GetAsync(path, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return new ContentResult
        {
            Content = content,
            ContentType = response.Content.Headers.ContentType?.ToString() ?? "application/json",
            StatusCode = (int)response.StatusCode
        };
    }
}
