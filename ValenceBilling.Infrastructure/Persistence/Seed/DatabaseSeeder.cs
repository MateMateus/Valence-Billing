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
        // Ponto 3: Captura única e atômica de tempo UTC para toda a execução
        var now = DateTime.UtcNow;

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

        // 2. Planos Comerciais (Resiliente, plano a plano e reativando se inativo)
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
                .FirstOrDefaultAsync(p => p.Nome == plano.Nome);

            if (planoExistente == null)
            {
                await context.Planos.AddAsync(plano);
            }
            else if (!planoExistente.IsActive)
            {
                planoExistente.IsActive = true;
            }
        }
        await context.SaveChangesAsync();

        // 3. Cliente de Teste B2B (Garante existência e estado ativo)
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
        else if (!clienteTeste.IsActive)
        {
            clienteTeste.Ativar();
            await context.SaveChangesAsync();
        }

        // 4. Usuários Iniciais (Com bypass de filtro global e garantia de role)
        await SeedUserAsync(context, userManager, "admin@billing.local", "Admin@123", "Administrador Valence", "Admin");
        await SeedUserAsync(context, userManager, "financeiro@billing.local", "Financeiro@123", "Operador Financeiro", "Financeiro");
        await SeedUserAsync(context, userManager, "empresa.teste@cliente.local", "Cliente@123", "Gestor Cliente Teste", "Cliente", clienteTeste.Id);

        // 5. Carga de Teste: Assinatura Ativa e Fatura Consistente
        var planoPro = await context.Planos
            .IgnoreQueryFilters()
            .FirstAsync(p => p.Nome == "Pro");

        // Ponto 2: Garante que a assinatura está ativa
        var assinaturaAtiva = await context.Assinaturas
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.ClienteId == clienteTeste.Id
                                   && a.PlanoId == planoPro.Id
                                   && a.Status == AssinaturaStatus.Ativa);

        if (assinaturaAtiva == null)
        {
            assinaturaAtiva = new Assinatura
            {
                ClienteId = clienteTeste.Id,
                PlanoId = planoPro.Id,
                Status = AssinaturaStatus.Ativa,
                DataInicio = now.AddMonths(-1),
                DataFimPeriodoAtual = now.AddMonths(1),
                DiaVencimento = 10
            };

            await context.Assinaturas.AddAsync(assinaturaAtiva);
            await context.SaveChangesAsync();
        }

        // Ponto 3: Competência e Vencimento calculados com consistência
        var competenciaAtual = now.ToString("yyyy-MM");
        var faturaAtual = await context.Faturas
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(f => f.AssinaturaId == assinaturaAtiva.Id && f.Competencia == competenciaAtual);

        if (faturaAtual == null)
        {
            var dataVencimento = new DateTime(now.Year, now.Month, 10, 0, 0, 0, DateTimeKind.Utc);

            // Se hoje já passou do dia 10, a fatura nasce como Vencida; senão, Pendente
            var statusFatura = now.Date > dataVencimento.Date ? FaturaStatus.Vencida : FaturaStatus.Pendente;

            faturaAtual = new Fatura
            {
                AssinaturaId = assinaturaAtiva.Id,
                ClienteId = assinaturaAtiva.ClienteId,
                Competencia = competenciaAtual,
                ValorTotal = planoPro.ValorMensal,
                DataVencimento = dataVencimento,
                Status = statusFatura
            };

            await context.Faturas.AddAsync(faturaAtual);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedUserAsync(
        AppDbContext context,
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string nomeCompleto,
        string role,
        Guid? clienteId = null)
    {
        var normalizedEmail = email.ToUpperInvariant();

        // Ponto 1: Localiza mesmo se o usuário estiver desativado por soft delete
        var user = await context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);

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
        }
        else if (!user.IsActive)
        {
            // Reativa o usuário se estava inativo
            user.IsActive = true;
            await context.SaveChangesAsync();
        }

        // Ponto 1: Garante que a role está atribuída mesmo se o usuário já existia
        if (!await userManager.IsInRoleAsync(user, role))
        {
            var roleResult = await userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Falha ao adicionar role '{role}' ao usuário '{email}': {errors}");
            }
        }
    }
}
