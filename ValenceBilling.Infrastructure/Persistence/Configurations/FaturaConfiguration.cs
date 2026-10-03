using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ValenceBilling.Domain.Entities;

namespace ValenceBilling.Infrastructure.Persistence.Configurations;

public class FaturaConfiguration : IEntityTypeConfiguration<Fatura>
{
    public void Configure(EntityTypeBuilder<Fatura> builder)
    {
        builder.ToTable("Faturas");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Competencia)
            .HasMaxLength(7) // Formato "YYYY-MM" (ex: "2026-10")
            .IsRequired();

        // Precisão financeira
        builder.Property(f => f.ValorTotal)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(f => f.DataVencimento)
            .IsRequired();

        builder.Property(f => f.Status)
            .IsRequired();

        builder.Property(f => f.DataPagamento);

        builder.Property(f => f.CreatedAt)
            .IsRequired();

        // Índice Único Composto: impede cobrança duplicada no mesmo mês para a mesma assinatura
        builder.HasIndex(f => new { f.AssinaturaId, f.Competencia })
            .IsUnique();

        // Índice de Performance para a Régua de Cobrança (busca rápida por status e vencimento)
        builder.HasIndex(f => new { f.Status, f.DataVencimento });

        // Relacionamento com Assinatura (1 Assinatura -> N Faturas)
        builder.HasOne(f => f.Assinatura)
            .WithMany(a => a.Faturas)
            .HasForeignKey(f => f.AssinaturaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relacionamento com Cliente (1 Cliente -> N Faturas)
        builder.HasOne(f => f.Cliente)
            .WithMany(c => c.Faturas)
            .HasForeignKey(f => f.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
