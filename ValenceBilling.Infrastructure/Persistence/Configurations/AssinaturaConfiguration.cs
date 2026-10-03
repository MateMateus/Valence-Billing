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

        builder.HasOne(a => a.Cliente)
            .WithMany(c => c.Assinaturas)
            .HasForeignKey(a => a.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Plano)
            .WithMany(p => p.Assinaturas)
            .HasForeignKey(a => a.PlanoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}