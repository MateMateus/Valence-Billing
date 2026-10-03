using System;
using Microsoft.AspNetCore.Identity;

namespace ValenceBilling.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string NomeCompleto { get; set; } = default!;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    // Vínculo opcional com a empresa tomadora do serviço
    public Guid? ClienteId { get; set; }
}
