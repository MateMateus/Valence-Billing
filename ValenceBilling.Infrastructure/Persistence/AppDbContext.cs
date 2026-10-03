using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ValenceBilling.Domain.Entities;
using ValenceBilling.Infrastructure.Identity;

namespace ValenceBilling.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Plano> Planos => Set<Plano>();
    public DbSet<Assinatura> Assinaturas => Set<Assinatura>();
    public DbSet<Fatura> Faturas => Set<Fatura>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Aplica automaticamente todos os IEntityTypeConfiguration da pasta Configurations
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Filtro Global de Soft Delete para os usuários do Identity
        builder.Entity<ApplicationUser>()
            .HasQueryFilter(u => u.IsActive);

        // Relacionamento opcional entre ApplicationUser e Cliente (FK física com SetNull)
        builder.Entity<ApplicationUser>()
            .HasOne<Cliente>()
            .WithMany()
            .HasForeignKey(user => user.ClienteId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
