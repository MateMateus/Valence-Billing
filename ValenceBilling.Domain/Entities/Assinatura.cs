using System;
using System.Collections.Generic;
using ValenceBilling.Domain.Enums;

namespace ValenceBilling.Domain.Entities;

public class Assinatura
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    // Chaves estrangeiras e navegações
    public Guid ClienteId { get; init; }
    public Cliente Cliente { get; set; } = default!;

    public Guid PlanoId { get; set; }
    public Plano Plano { get; set; } = default!;

    public AssinaturaStatus Status { get; set; } = AssinaturaStatus.Pendente;
    public DateTime DataInicio { get; init; } = DateTime.UtcNow;
    public DateTime DataFimPeriodoAtual { get; set; }
    public int DiaVencimento { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    // 1 Assinatura -> N Faturas geradas ao longo do tempo
    public ICollection<Fatura> Faturas { get; set; } = new List<Fatura>();
}
