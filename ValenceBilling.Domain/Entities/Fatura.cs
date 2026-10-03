using System;
using ValenceBilling.Domain.Enums;

namespace ValenceBilling.Domain.Entities;

public class Fatura
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    // Vínculos
    public Guid AssinaturaId { get; init; }
    public Assinatura Assinatura { get; set; } = default!;

    public Guid ClienteId { get; init; }
    public Cliente Cliente { get; set; } = default!;

    // Dados Financeiros
    public string Competencia { get; init; } = default!; // Ex: "2026-10"
    public decimal ValorTotal { get; init; }
    public DateTime DataVencimento { get; set; }
    public FaturaStatus Status { get; set; } = FaturaStatus.Pendente;
    public DateTime? DataPagamento { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
