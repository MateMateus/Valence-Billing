using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ValenceBilling.Domain.Entities;

namespace ValenceBilling.Infrastructure.Persistence.Configurations;

public class PlanoConfiguration : IEntityTypeConfiguration<Plano>
{
    public void Configure(EntityTypeBuilder<Plano> builder)
    {
        builder.ToTable("Planos");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nome)
            .HasMaxLength(100)
            .IsRequired();

        // Nome do plano deve ser único no catálogo
        builder.HasIndex(p => p.Nome)
            .IsUnique();

        builder.Property(p => p.Descricao)
            .HasMaxLength(500);

        // Precisão de moeda (18 dígitos no total, 2 casas decimais)
        builder.Property(p => p.ValorMensal)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.LimiteUsuarios)
            .IsRequired();

        builder.Property(p => p.IsActive)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        // Filtro Global de Soft Delete
        builder.HasQueryFilter(p => p.IsActive);
    }
}
