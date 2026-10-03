using System;

namespace ValenceBilling.Domain.Entities;

public class Cliente
{
    // ID do cliente
    public Guid Id { get; init; } = Guid.CreateVersion7();

    // Dados do cliente
    public string RazaoSocial { get; init; } = default!;
    public string NomeFantasia { get; init; } = default!;
    public string Cnpj { get; init; } = default!;
    public string EmailFinanceiro { get; init; } = default!;
    public string Telefone { get; init; } = default!;

    // Status do cliente
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    // Propriedade de navegação (1 Cliente -> N Assinaturas, N Faturas)
    public ICollection<Assinatura> Assinaturas { get; set; } = new List<Assinatura>();
    public ICollection<Fatura> Faturas { get; set; } = new List<Fatura>();
}
