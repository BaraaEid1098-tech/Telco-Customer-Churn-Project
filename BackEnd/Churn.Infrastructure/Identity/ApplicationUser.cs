using Microsoft.AspNetCore.Identity;

namespace Churn.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
