using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ValenceBilling.Domain.Entities;

namespace ValenceBilling.Infrastructure.Persistence.Configurations;

public class AssinaturaConfiguration : IEntityTypeConfiguration<Assinatura>
{
    public void Configure(EntityTypeBuilder<Assinatura> builder)
    {
        builder.ToTable("Assinaturas");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Status)
            .IsRequired();

        builder.Property(a => a.DataInicio)
            .IsRequired();

        builder.Property(a => a.DataFimPeriodoAtual)
            .IsRequired();

        builder.Property(a => a.DiaVencimento)
            .IsRequired();

        builder.Property(a => a.CreatedAt)
            .IsRequired();

        // Relacionamento com Cliente (1 Cliente -> N Assinaturas)
        builder.HasOne(a => a.Cliente)
            .WithMany(c => c.Assinaturas)
            .HasForeignKey(a => a.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relacionamento com Plano (1 Plano -> N Assinaturas)
        builder.HasOne(a => a.Plano)
            .WithMany(p => p.Assinaturas)
            .HasForeignKey(a => a.PlanoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relacionamento com Faturas (1 Assinatura -> N Faturas)
        builder.HasMany(a => a.Faturas)
            .WithOne(f => f.Assinatura)
            .HasForeignKey(f => f.AssinaturaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
