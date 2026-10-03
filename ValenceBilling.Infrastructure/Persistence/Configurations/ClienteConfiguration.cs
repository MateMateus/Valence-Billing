using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ValenceBilling.Domain.Entities;

namespace ValenceBilling.Infrastructure.Persistence.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.RazaoSocial)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(c => c.NomeFantasia)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Cnpj)
            .HasMaxLength(14)
            .IsRequired();

        // Constraint de unicidade no CNPJ
        builder.HasIndex(c => c.Cnpj)
            .IsUnique();

        builder.Property(c => c.EmailFinanceiro)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(c => c.Telefone)
            .HasMaxLength(11) // DDD (2) + Número (8 ou 9 dígitos limpos)
            .IsRequired();

        builder.Property(c => c.IsActive)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        // Filtro Global de Soft Delete: ignora registros desativados nas consultas
        builder.HasQueryFilter(c => c.IsActive);
    }
}
