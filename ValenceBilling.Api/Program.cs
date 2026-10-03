using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ValenceBilling.Infrastructure.Identity;
using ValenceBilling.Infrastructure.Persistence;
using ValenceBilling.Infrastructure.Persistence.Seed;

var builder = WebApplication.CreateBuilder(args);

// 1. Connection String e DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não encontrada.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Identity com chaves Guid
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddOpenApi();

var app = builder.Build();

// 3. Execução de Migrations e Seed Idempotente em Desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        // Aplica migrations pendentes no banco automaticamente
        await context.Database.MigrateAsync();

        // Executa o Seeder idempotente
        await DatabaseSeeder.SeedAsync(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogCritical(ex, "Falha crítica ao migrar ou popular o banco de dados. O startup da API será interrompido.");
        throw; // Interrompe a execução imediatamente
    }
}

app.UseHttpsRedirection();

app.Run();
