using System;
using System.Collections.Generic;

namespace ValenceBilling.Domain.Entities;

public class Plano
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public string Nome { get; set; } = default!;
    public string? Descricao { get; set; }
    public decimal ValorMensal { get; set; }
    public int LimiteUsuarios { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    // Propriedade de navegação (1 Plano -> N Assinaturas)
    public ICollection<Assinatura> Assinaturas { get; set; } = new List<Assinatura>();
}
