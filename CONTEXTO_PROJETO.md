# Contexto do Projeto: Motor de Faturamento Recorrente B2B (Billing Engine)

> **Documento Vivo de Arquitetura e Contexto**  
> Este arquivo reúne todo o escopo, arquitetura, decisões técnicas, regras de trabalho e o progresso atual do projeto. Qualquer IA ou desenvolvedor deve ler este documento antes de sugerir ou continuar o desenvolvimento.

---

## 1. Visão Geral e Objetivo do Sistema

* **Projeto:** Motor de Faturamento Recorrente e Cobrança B2B (SaaS / Fintech).
* **Solução:** `ValenceBilling.sln`
* **Objetivo:** Sistema de alta complexidade corporativa para gerenciar planos de assinatura, clientes corporativos (B2B), contratos de recorrência, emissão automatizada de faturas e controle de inadimplência/pagamentos.
* **Nível:** Projetado com padrões e exigências de processos seletivos plenos/seniores (concorrência, integridade referencial, imutabilidade financeira, auditoria e Clean Architecture).

---

## 2. Stack Tecnológica e Ferramentas

| Tecnologia | Função no Sistema | Status |
| :--- | :--- | :--- |
| **.NET 9 (C#)** | Plataforma principal da API e Web | Configurado (`global.json` e `.csproj`) |
| **Entity Framework Core 9** | ORM, Migrations e Fluent API | Instalado no `ValenceBilling.Infrastructure` |
| **SQL Server** | Banco de dados relacional | Provedor configurado |
| **ASP.NET Core Identity** | Autenticação, autorização e perfis | Configurado com chaves `Guid` |
| **Bootstrap 5.3** | Interface web responsiva | Instalado no `ValenceBilling.Web/wwwroot` |
| **JavaScript (Fetch API)** | Comunicação assíncrona com a API | Nativo do navegador |
| **Chart.js** | Gráficos do dashboard financeiro | Será importado via CDN nas telas Razor |

---

## 3. Arquitetura e Divisão das Camadas (Clean Architecture)

A solução segue a separação estrita de responsabilidades:

1. **`ValenceBilling.Domain` (Coração do Negócio):**
   * Totalmente puro e agnóstico de frameworks/bancos de dados.
   * Contém as entidades, enums, regras de negócio e invariantes.
   * **Sem DataAnnotations** (o mapeamento de banco é feito exclusivamente via Fluent API na infraestrutura).

2. **`ValenceBilling.Application`:**
   * Casos de uso, DTOs, interfaces de serviços, regras de aplicação e validações de entrada (FluentValidation).

3. **`ValenceBilling.Infrastructure`:**
   * Acesso a dados (`AppDbContext`), mapeamentos Fluent API, Identity (`ApplicationUser`), Migrations e Seeds idempotentes.

4. **`ValenceBilling.Api`:**
   * Endpoints RESTful para integração externa, processamento assíncrono e OpenAPI/Swagger.

5. **`ValenceBilling.Web`:**
   * Aplicação MVC (Razor Views + Controllers) para o painel administrativo/backoffice e dashboards financeiros.

---

## 4. Regras de Trabalho Mandatórias (Instruções para a IA)

> [!IMPORTANT]
> 1. **O desenvolvedor escreve TODO o código na mão.**
>    * A IA atua **exclusivamente** como arquiteta de software, mentora técnica e revisora.
>    * A IA **NÃO deve criar nem modificar arquivos de código C# diretamente no disco**, a menos que o usuário solicite explicitamente.
> 2. **Didática e Passo a Passo:**
>    * Explicar o porquê de cada decisão técnica, apresentar o código como modelo para o usuário digitar, validar o que o usuário criou e apontar correções.
> 3. **Atualização Contínua do Contexto:**
>    * Ao concluir marcos ou decisões importantes, a IA deve orientar a atualização deste arquivo `CONTEXTO_PROJETO.md`.

---

## 5. Modelo de Domínio (Agregados Principais)

1. **`Cliente`:**
   * Empresas B2B tomadoras do serviço.
   * Campos: `Id` (`Guid.CreateVersion7`), `RazaoSocial`, `NomeFantasia`, `Cnpj` (único, 14 dígitos), `EmailFinanceiro`, `Telefone`, `IsActive`, `CreatedAt`.
   * Navegações: `ICollection<Assinatura> Assinaturas`, `ICollection<Fatura> Faturas`.
2. **`Plano`:**
   * Planos de assinatura comercializados (ex: Starter, Pro, Enterprise).
   * Campos: `Id` (`Guid.CreateVersion7`), `Nome` (único), `Descricao`, `ValorMensal` (`decimal`), `LimiteUsuarios`, `IsActive`, `CreatedAt`.
   * Navegações: `ICollection<Assinatura> Assinaturas`.
3. **`Assinatura`:**
   * Contrato recorrente que vincula um `Cliente` a um `Plano`.
   * Campos: `Id` (`Guid.CreateVersion7`), `ClienteId` (FK), `Cliente`, `PlanoId` (FK), `Plano`, `Status` (`AssinaturaStatus`), `DataInicio`, `DataFimPeriodoAtual`, `DiaVencimento`, `CreatedAt`.
   * Navegações: `ICollection<Fatura> Faturas`.
4. **`Fatura`:**
   * Registro financeiro imutável gerado por ciclo de cobrança.
   * Campos: `Id` (`Guid.CreateVersion7`), `AssinaturaId` (FK), `Assinatura`, `ClienteId` (FK), `Cliente`, `Competencia` (Mês/Ano ex: "2026-10"), `ValorTotal` (`decimal`), `DataVencimento`, `Status` (`FaturaStatus`), `DataPagamento`, `CreatedAt`.

### Enums do Domínio (`ValenceBilling.Domain/Enums`):
* **`AssinaturaStatus`:** `Pendente = 1`, `Ativa = 2`, `Suspensa = 3`, `Cancelada = 4`.
* **`FaturaStatus`:** `Pendente = 1`, `Paga = 2`, `Vencida = 3`, `Cancelada = 4`.

---

## 6. Fase Atual do Projeto: Fase 1 (Modelagem e Persistência)

### **Objetivo da Fase 1:**
Modelar entidades e enums, configurar EF Core com Fluent API, configurar ASP.NET Identity com `Guid`, preparar migrações e seed idempotente no SQL Server.

### **Entregáveis Técnicos:**
* **Integridade Referencial:**
  * Relacionamentos com `DeleteBehavior.Restrict` em todas as chaves estrangeiras de `Assinatura` e `Fatura`.
* **Índices de Performance e Constraints:**
  * `Cliente(Cnpj)` único (14 dígitos).
  * `Plano(Nome)` único.
  * `Fatura(AssinaturaId, Competencia)` composto único (evita cobrança dupla no mesmo ciclo).
  * `Fatura(Status, DataVencimento)` para otimização da régua de cobrança.
* **Filtros Globais de Consulta (Soft Delete):**
  * `IsActive = true` em `Cliente`, `Plano` e `ApplicationUser`.
* **Segurança e Perfis de Acesso (Identity):**
  * `ApplicationUser : IdentityUser<Guid>`
  * Perfis (Roles): `Admin`, `Financeiro`, `Cliente`.
* **Seed Idempotente:**
  * População de Roles, Usuários padrão (`admin@billing.local`, `financeiro@billing.local`, `empresa.teste@cliente.local`), catálogo de 3 planos e 1 carga de teste completa sem duplicar registros caso o sistema reinicie.

### **Comandos de Execução:**
```powershell
# 1. Gerar Migration
dotnet ef migrations add InitialCreate -p ValenceBilling.Infrastructure -s ValenceBilling.Api -o Persistence/Migrations

# 2. Atualizar Banco de Dados
dotnet ef database update -p ValenceBilling.Infrastructure -s ValenceBilling.Api

# 3. Executar API e Seed
dotnet run --project ValenceBilling.Api
```

---

## 7. Status Atual das Pastas e Arquivos

### **Domínio (`ValenceBilling.Domain`) - CONCLUÍDO (0 Erros / 0 Avisos)**:
* **`ValenceBilling.Domain/Entities/`:**
  * [x] `Cliente.cs` - Implementado (Guid v7, soft delete, coleções de navegação)
  * [x] `Plano.cs` - Implementado (Guid v7, valor mensal decimal, navegação)
  * [x] `Assinatura.cs` - Implementado (Guid v7, FKs explícitas, ciclo de faturamento)
  * [x] `Fatura.cs` - Implementado (Guid v7, competência imutável, status, FKs)
* **`ValenceBilling.Domain/Enums/`:**
  * [x] `AssinaturaStatus.cs` - Implementado
  * [x] `FaturaStatus.cs` - Implementado

### **Infraestrutura (`ValenceBilling.Infrastructure`) - EM ANDAMENTO**:
* **`ValenceBilling.Infrastructure/Identity/`:**
  * [ ] `ApplicationUser.cs` - Próximo passo (herdar `IdentityUser<Guid>`)
* **`ValenceBilling.Infrastructure/Persistence/`:**
  * [ ] `AppDbContext.cs`
  * [ ] `DesignTimeDbContextFactory.cs`
  * [ ] Pastas `Configurations/` (Fluent API para cada entidade)
  * [ ] Pastas `Migrations/` e `Seed/`

### **Próximos Passos:**
1. Implementar `ApplicationUser.cs` herdando de `IdentityUser<Guid>`.
2. Implementar as configurações Fluent API (`IEntityTypeConfiguration<T>`).
3. Configurar `AppDbContext` herdando de `IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>`.
4. Configurar `DesignTimeDbContextFactory.cs`.
5. Gerar e aplicar a primeira Migration (`InitialCreate`).
6. Criar o Seeder idempotente (`DatabaseSeeder.cs`).

---

## 8. Decisões Arquiteturais Registradas

1. **Adoção do UUIDv7 (`Guid.CreateVersion7()` do .NET 9):**
   * Decisão: Utilizar `Guid.CreateVersion7()` como valor padrão para todas as chaves primárias.
   * Motivo: Combina um prefixo temporal sequencial com entropia aleatória. Isso resolve 100% o problema clássico de fragmentação de índice B-Tree no SQL Server (mantendo performance equivalente a inteiros) e simultaneamente previne ataques de enumeração/IDOR e vazamento de métricas corporativas, além de permitir geração de ID em memória no cliente antes de persistir no banco.
2. **Separação de Responsabilidades e Validações:**
   * Domínio: POCOs puros, sem DataAnnotations.
   * Banco de dados: Constraints físicas de tamanho (`HasMaxLength(14)` para CNPJ, `HasMaxLength(20)` para Telefone) e índices únicos via Fluent API.
   * Regras de negócio: Validações de formato, dígitos verificadores de CNPJ e consistência via camada `Application` (FluentValidation).
3. **Imutabilidade Financeira:**
   * As faturas registram competência e valor total fixados no momento da cobrança para garantir rastreabilidade contábil imutável.
