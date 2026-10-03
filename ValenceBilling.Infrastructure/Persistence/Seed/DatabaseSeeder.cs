using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ValenceBilling.Domain.Entities;
using ValenceBilling.Domain.Enums;
using ValenceBilling.Infrastructure.Identity;

namespace ValenceBilling.Infrastructure.Persistence.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        AppDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        // 1. Roles
        string[] roles = ["Admin", "Financeiro", "Cliente"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole<Guid>
                {
                    Name = role,
                    NormalizedName = role.ToUpperInvariant()
                });

                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Falha ao criar role '{role}': {errors}");
                }
            }
        }

        // 2. Planos Comerciais (Inserção individual e resiliente com IgnoreQueryFilters)
        var catalogoPlanos = new[]
        {
            new Plano
            {
                Nome = "Starter",
                Descricao = "Plano ideal para pequenas empresas em fase inicial",
                ValorMensal = 99.00m,
                LimiteUsuarios = 5
            },
            new Plano
            {
                Nome = "Pro",
                Descricao = "Plano para empresas em expansão e equipes estruturadas",
                ValorMensal = 199.00m,
                LimiteUsuarios = 20
            },
            new Plano
            {
                Nome = "Enterprise",
                Descricao = "Plano corporativo completo com suporte prioritário",
                ValorMensal = 499.00m,
                LimiteUsuarios = 100
            }
        };

        foreach (var plano in catalogoPlanos)
        {
            var planoExistente = await context.Planos
                .IgnoreQueryFilters()
                .AnyAsync(p => p.Nome == plano.Nome);

            if (!planoExistente)
            {
                await context.Planos.AddAsync(plano);
            }
        }
        await context.SaveChangesAsync();

        // 3. Cliente de Teste B2B (CNPJ matematicamente válido na Receita Federal)
        var cnpjTeste = "45997418000153";
        var clienteTeste = await context.Clientes
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Cnpj == cnpjTeste);

        if (clienteTeste == null)
        {
            clienteTeste = new Cliente
            {
                RazaoSocial = "Empresa de Teste B2B LTDA",
                NomeFantasia = "Teste Corp",
                Cnpj = cnpjTeste,
                EmailFinanceiro = "financeiro@testecorp.local",
                Telefone = "11987654321"
            };

            await context.Clientes.AddAsync(clienteTeste);
            await context.SaveChangesAsync();
        }

        // 4. Usuários Iniciais com Validação de Erros
        await SeedUserAsync(userManager, "admin@billing.local", "Admin@123", "Administrador Valence", "Admin");
        await SeedUserAsync(userManager, "financeiro@billing.local", "Financeiro@123", "Operador Financeiro", "Financeiro");
        await SeedUserAsync(userManager, "empresa.teste@cliente.local", "Cliente@123", "Gestor Cliente Teste", "Cliente", clienteTeste.Id);

        // 5. Carga de Teste: Assinatura e Fatura com Paridade de ClienteId
        var planoPro = await context.Planos
            .IgnoreQueryFilters()
            .FirstAsync(p => p.Nome == "Pro");

        var assinaturaAtiva = await context.Assinaturas
            .FirstOrDefaultAsync(a => a.ClienteId == clienteTeste.Id && a.PlanoId == planoPro.Id);

        if (assinaturaAtiva == null)
        {
            assinaturaAtiva = new Assinatura
            {
                ClienteId = clienteTeste.Id,
                PlanoId = planoPro.Id,
                Status = AssinaturaStatus.Ativa,
                DataInicio = DateTime.UtcNow.AddMonths(-1),
                DataFimPeriodoAtual = DateTime.UtcNow.AddMonths(1),
                DiaVencimento = 10
            };

            await context.Assinaturas.AddAsync(assinaturaAtiva);
            await context.SaveChangesAsync();
        }

        // Fatura da competência atual garantindo paridade de ClienteId
        var competenciaAtual = DateTime.UtcNow.ToString("yyyy-MM");
        var faturaAtual = await context.Faturas
            .FirstOrDefaultAsync(f => f.AssinaturaId == assinaturaAtiva.Id && f.Competencia == competenciaAtual);

        if (faturaAtual == null)
        {
            faturaAtual = new Fatura
            {
                AssinaturaId = assinaturaAtiva.Id,
                ClienteId = assinaturaAtiva.ClienteId, // Garante que a fatura e a assinatura apontam para o mesmo cliente
                Competencia = competenciaAtual,
                ValorTotal = planoPro.ValorMensal,
                DataVencimento = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 10, 0, 0, 0, DateTimeKind.Utc),
                Status = FaturaStatus.Pendente
            };

            await context.Faturas.AddAsync(faturaAtual);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string nomeCompleto,
        string role,
        Guid? clienteId = null)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                NomeCompleto = nomeCompleto,
                ClienteId = clienteId
            };

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Falha ao criar usuário '{email}': {errors}");
            }

            var roleResult = await userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Falha ao adicionar role '{role}' ao usuário '{email}': {errors}");
            }
        }
    }
}
