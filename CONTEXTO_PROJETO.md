# Contexto do Projeto: Motor de Faturamento Recorrente B2B (Billing Engine)

> **Documento Vivo de Arquitetura e Contexto**  
> Este arquivo reÃºne todo o escopo, arquitetura, decisÃµes tÃ©cnicas, regras de trabalho e o progresso atual do projeto. Qualquer IA ou desenvolvedor deve ler este documento antes de sugerir ou continuar o desenvolvimento.

---

## 1. VisÃ£o Geral e Objetivo do Sistema

* **Projeto:** Motor de Faturamento Recorrente e CobranÃ§a B2B (SaaS / Fintech).
* **SoluÃ§Ã£o:** `ValenceBilling.sln`
* **Objetivo:** Sistema de alta complexidade corporativa para gerenciar planos de assinatura, clientes corporativos (B2B), contratos de recorrÃªncia, emissÃ£o automatizada de faturas e controle de inadimplÃªncia/pagamentos.
* **NÃ­vel:** Projetado com padrÃµes e exigÃªncias de processos seletivos plenos/seniores (concorrÃªncia, integridade referencial, imutabilidade financeira, auditoria e Clean Architecture).

---

## 2. Stack TecnolÃ³gica e Ferramentas

| Tecnologia | FunÃ§Ã£o no Sistema | Status |
| :--- | :--- | :--- |
| **.NET 9 (C#)** | Plataforma principal da API e Web | Configurado (`global.json` e `.csproj`) |
| **Entity Framework Core 9** | ORM, Migrations e Fluent API | Instalado no `ValenceBilling.Infrastructure` |
| **SQL Server** | Banco de dados relacional | Provedor configurado |
| **ASP.NET Core Identity** | AutenticaÃ§Ã£o, autorizaÃ§Ã£o e perfis | Configurado com chaves `Guid` |
| **Bootstrap 5.3** | Interface web responsiva | Instalado no `ValenceBilling.Web/wwwroot` |
| **JavaScript (Fetch API)** | ComunicaÃ§Ã£o assÃ­ncrona com a API | Nativo do navegador |
| **Chart.js** | GrÃ¡ficos do dashboard financeiro | SerÃ¡ importado via CDN nas telas Razor |

---

## 3. Arquitetura e DivisÃ£o das Camadas (Clean Architecture)

A soluÃ§Ã£o segue a separaÃ§Ã£o estrita de responsabilidades:

1. **`ValenceBilling.Domain` (CoraÃ§Ã£o do NegÃ³cio):**
   * Totalmente puro e agnÃ³stico de frameworks/bancos de dados.
   * ContÃ©m as entidades, regras de negÃ³cio e invariantes.
   * **Sem DataAnnotations** (o mapeamento de banco Ã© feito exclusivamente via Fluent API na infraestrutura).

2. **`ValenceBilling.Application`:**
   * Casos de uso, DTOs, interfaces de serviÃ§os e regras de aplicaÃ§Ã£o.

3. **`ValenceBilling.Infrastructure`:**
   * Acesso a dados (`AppDbContext`), mapeamentos Fluent API, Identity (`ApplicationUser`), Migrations e Seeds idempotentes.

4. **`ValenceBilling.Api`:**
   * Endpoints RESTful para integraÃ§Ã£o externa, processamento assÃ­ncrono e OpenAPI/Swagger.

5. **`ValenceBilling.Web`:**
   * AplicaÃ§Ã£o MVC (Razor Views + Controllers) para o painel administrativo/backoffice e dashboards financeiros.

---

## 4. Regras de Trabalho MandatÃ³rias (InstruÃ§Ãµes para a IA)

> [!IMPORTANT]
> 1. **O desenvolvedor escreve TODO o cÃ³digo na mÃ£o.**
>    * A IA atua **exclusivamente** como arquiteta de software, mentora tÃ©cnica e revisora.
>    * A IA **NÃƒO deve criar nem modificar arquivos de cÃ³digo C# diretamente no disco**, a menos que o usuÃ¡rio solicite explicitamente.
> 2. **DidÃ¡tica e Passo a Passo:**
>    * Explicar o porquÃª de cada decisÃ£o tÃ©cnica, apresentar o cÃ³digo como modelo para o usuÃ¡rio digitar, validar o que o usuÃ¡rio criou e apontar correÃ§Ãµes.
> 3. **AtualizaÃ§Ã£o ContÃ­nua do Contexto:**
>    * Ao concluir marcos ou decisÃµes importantes, a IA deve orientar a atualizaÃ§Ã£o deste arquivo `CONTEXTO_PROJETO.md`.

---

## 5. Modelo de DomÃ­nio (Agregados Principais)

1. **`Plano`:**
   * Representa os planos de assinatura comercializados (ex: Starter, Pro, Enterprise).
   * Campos: `Id`, `Nome` (Ãºnico), `Descricao`, `ValorMensal`, `Frequencia`, `LimiteUsuarios`, `IsActive`, `CreatedAt`.
2. **`Cliente`:**
   * Empresas B2B tomadoras do serviÃ§o.
   * Campos: `Id`, `RazaoSocial`, `NomeFantasia`, `Cnpj` (Ãºnico), `EmailFinanceiro`, `Telefone`, `IsActive`, `CreatedAt`.
3. **`Assinatura`:**
   * Contrato recorrente que vincula um `Cliente` a um `Plano`.
   * Campos: `Id`, `ClienteId`, `PlanoId`, `Status` (Ativa, Pendente, Suspensa, Cancelada), `DataInicio`, `DataFimPeriodoAtual`, `DiaVencimento`, `CreatedAt`.
4. **`Fatura`:**
   * Registro financeiro imutÃ¡vel gerado por ciclo de cobranÃ§a.
   * Campos: `Id`, `AssinaturaId`, `ClienteId`, `Competencia` (MÃªs/Ano), `ValorTotal`, `DataVencimento`, `Status` (Pendente, Paga, Vencida, Cancelada), `DataPagamento`, `CreatedAt`.

---

## 6. Fase Atual do Projeto: Fase 1

### **Objetivo da Fase 1:**
Modelar entidades, configurar EF Core com Fluent API, configurar ASP.NET Identity com `Guid`, preparar migraÃ§Ãµes e seed idempotente no SQL Server.

### **EntregÃ¡veis TÃ©cnicos:**
* **Integridade Referencial:**
  * Relacionamentos com `DeleteBehavior.Restrict` em todas as chaves estrangeiras de `Assinatura` e `Fatura`.
* **Ãndices de Performance e Constraints:**
  * `Cliente(Cnpj)` Ãºnico.
  * `Plano(Nome)` Ãºnico.
  * `Fatura(AssinaturaId, Competencia)` composto Ãºnico.
  * `Fatura(Status, DataVencimento)` para otimizaÃ§Ã£o da rÃ©gua de cobranÃ§a.
* **Filtros Globais de Consulta (Soft Delete):**
  * `IsActive = true` em `Cliente`, `Plano` e `ApplicationUser`.
* **SeguranÃ§a e Perfis de Acesso (Identity):**
  * `ApplicationUser : IdentityUser<Guid>`
  * Perfis (Roles): `Admin`, `Financeiro`, `Cliente`.
* **Seed Idempotente:**
  * PopulaÃ§Ã£o de Roles, UsuÃ¡rios padrÃ£o (`admin@billing.local`, `financeiro@billing.local`, `empresa.teste@cliente.local`), catÃ¡logo de 3 planos e 1 carga de teste completa sem duplicar registros caso o sistema reinicie.

### **Comandos de ExecuÃ§Ã£o:**
```powershell
# 1. Gerar Migration
dotnet ef migrations add InitialCreate -p ValenceBilling.Infrastructure -s ValenceBilling.Api -o Persistence/Migrations

# 2. Atualizar Banco de Dados
dotnet ef database update -p ValenceBilling.Infrastructure -s ValenceBilling.Api

# 3. Executar API e Seed
dotnet run --project ValenceBilling.Api
```

---

## 7. Status Atual das Pastas e Arquivos Criados

* **`ValenceBilling.Domain/Entities/`:**
  * [x] `Plano.cs`
  * [x] `Cliente.cs`
  * [x] `Assinatura.cs`
  * [x] `Fatura.cs`
* **`ValenceBilling.Infrastructure/Identity/`:**
  * [x] `ApplicationUser.cs`
* **`ValenceBilling.Infrastructure/Persistence/`:**
  * [x] `AppDbContext.cs`
  * [x] `DesignTimeDbContextFactory.cs`
  * [x] Pastas `Migrations/` e `Seed/` criadas
* **PrÃ³ximos Passos:**
  1. Validar e preencher as propriedades e regras nas entidades do `Domain`.
  2. Implementar as configuraÃ§Ãµes Fluent API (`IEntityTypeConfiguration<T>`).
  3. Configurar `AppDbContext` herdando de `IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>`.
  4. Implementar o `DatabaseSeeder.cs`.

### 7.1 DecisÃµes de Estrutura e Entidades

- **Nomenclatura e pastas**: Mantidas como `Domain/Entities`, `Infrastructure/Identity` e `Infrastructure/Persistence`, seguindo a Clean Architecture.
- **Entidades**: Cada classe (`Plano`, `Cliente`, `Assinatura`, `Fatura`) contÃ©m apenas as propriedades de domÃ­nio.  
  - NÃ£o hÃ¡ lÃ³gica de validaÃ§Ã£o interna â€“ serÃ¡ tratada na camada **Application** (useâ€‘cases / services).  
  - Propriedades `Id` usam `Guid`, `CreatedAt` em UTC, `IsActive` para softâ€‘delete.  
- **Enums**: Foram adicionados `AssinaturaStatus` e `FaturaStatus` em `Domain/Enums` para representar os estados de ciclo de vida.

### 7.2 EstratÃ©gia de implementaÃ§Ã£o das entidades

- **Cliente.cs** serÃ¡ implementado como **POCO simples** â€“ classe pÃºblica com propriedades autoâ€‘implementadas (`set` pÃºblico).  
- As validaÃ§Ãµes de CNPJ, eâ€‘mail, etc., serÃ£o tratadas na camada **Application** (services / useâ€‘cases), nÃ£o dentro da prÃ³pria entidade.  
- As demais entidades (`Plano`, `Assinatura`, `Fatura`) seguirÃ£o o mesmo padrÃ£o, permitindo foco na modelagem de domÃ­nio antes da camada de persistÃªncia.

