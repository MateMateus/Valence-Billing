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
| **Entity Framework Core 9** | ORM, Migrations e Fluent API | Instalado e configurado |
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
   * Contém as entidades, enums, regras de negócio e invariantes (DDD: métodos explícitos de transição de estado como `Ativar()` e `Desativar()`).
   * **Sem DataAnnotations** (o mapeamento de banco é feito exclusivamente via Fluent API na infraestrutura).

2. **`ValenceBilling.Application`:**
   * Casos de uso, DTOs, interfaces de serviços, regras de aplicação e validações de entrada (FluentValidation).

3. **`ValenceBilling.Infrastructure`:**
   * Acesso a dados (`AppDbContext`), mapeamentos Fluent API, Identity (`ApplicationUser`), Migrations e Seeds idempotentes com resiliência total.

4. **`ValenceBilling.Api`:**
   * Endpoints RESTful para integração externa, processamento assíncrono, OpenAPI/Swagger e pipeline de execução automática de migrations e seed em desenvolvimento.

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
   * Campos: `Id` (`Guid.CreateVersion7`), `RazaoSocial`, `NomeFantasia`, `Cnpj` (único, 14 dígitos), `EmailFinanceiro`, `Telefone` (11 dígitos limpos), `IsActive` (`private set`), `CreatedAt`.
   * Métodos DDD: `Ativar()`, `Desativar()`.
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

## 6. Fase 1: Modelagem, Persistência e Infraestrutura

### **Entregáveis Técnicos Concluídos:**
* **Integridade Referencial e Mapeamentos Limpos:**
  * Relacionamentos centralizados unicamente nas entidades dependentes (`AssinaturaConfiguration` e `FaturaConfiguration`).
  * `DeleteBehavior.Restrict` em todas as chaves estrangeiras de `Assinatura` e `Fatura`.
  * `ApplicationUser` com FK opcional para `Cliente` (`DeleteBehavior.SetNull`).
* **Índices de Performance e Constraints:**
  * `Cliente(Cnpj)` único (14 dígitos numéricos sanitizados).
  * `Cliente(Telefone)` limitado a 11 dígitos numéricos sanitizados.
  * `Plano(Nome)` único.
  * `Fatura(AssinaturaId, Competencia)` composto único (evita cobrança duplicada no mesmo ciclo).
  * `Fatura(Status, DataVencimento)` para otimização da régua de cobrança.
* **Filtros Globais de Consulta (Soft Delete):**
  * `IsActive = true` em `Cliente`, `Plano` e `ApplicationUser`.
  * Regra contábil: Consultas de relatórios históricos e fiscais devem aplicar `.IgnoreQueryFilters()` para não ocultar clientes inativos de faturas passadas.
* **Segurança e Perfis de Acesso (Identity):**
  * `ApplicationUser : IdentityUser<Guid>`
  * Perfis (Roles): `Admin`, `Financeiro`, `Cliente`.
* **Seed Idempotente e Resiliente:**
  * População plano a plano tolerante a estados parciais.
  * CNPJ de teste com algoritmo matematicamente válido (`45997418000153`).
  * Uso de `.IgnoreQueryFilters()` no Seeder para evitar colisões com registros desativados.
  * Tratamento estrito de erros do `IdentityResult` e reativação automática de entidades de teste.
  * Cálculo consistente de vencimento a partir de captura atômica de `DateTime.UtcNow`.
* **Integração com a API (`Program.cs`):**
  * Startup configurado para aplicar `context.Database.MigrateAsync()` e invocar o `DatabaseSeeder` em ambiente de desenvolvimento.
  * Falhas críticas de inicialização interrompem a API (`throw`) para impedir falsos positivos.

---

## 7. Status das Camadas e Arquivos

### **Domínio (`ValenceBilling.Domain`) - CONCLUÍDO (0 Erros / 0 Avisos)**:
* [x] `Entities/Cliente.cs` (Guid v7, DDD com Ativar/Desativar, soft delete)
* [x] `Entities/Plano.cs` (Guid v7, valor mensal decimal, navegação)
* [x] `Entities/Assinatura.cs` (Guid v7, FKs explícitas, ciclo de faturamento)
* [x] `Entities/Fatura.cs` (Guid v7, competência imutável, status, FKs)
* [x] `Enums/AssinaturaStatus.cs`
* [x] `Enums/FaturaStatus.cs`

### **Infraestrutura (`ValenceBilling.Infrastructure`) - CONCLUÍDO (0 Erros / 0 Avisos)**:
* [x] `Identity/ApplicationUser.cs`
* [x] `Persistence/Configurations/ClienteConfiguration.cs`
* [x] `Persistence/Configurations/PlanoConfiguration.cs`
* [x] `Persistence/Configurations/AssinaturaConfiguration.cs`
* [x] `Persistence/Configurations/FaturaConfiguration.cs`
* [x] `Persistence/AppDbContext.cs`
* [x] `Persistence/DesignTimeDbContextFactory.cs`
* [x] `Persistence/Seed/DatabaseSeeder.cs`
* [x] `Persistence/Migrations/20261003021527_InitialCreate.cs` (Migration gerada com sucesso)

### **API (`ValenceBilling.Api`) - INTEGRADA (0 Erros / 0 Avisos)**:
* [x] `appsettings.Development.json` (Connection String do SQL Server LocalDB)
* [x] `Program.cs` (Injeção de DbContext, Identity, Migrations e Seeder)
* [x] `ValenceBilling.Api.csproj` (Referência ao `Microsoft.EntityFrameworkCore.Design`)

---

## 8. Decisões Arquiteturais Registradas

1. **Adoção do UUIDv7 (`Guid.CreateVersion7()` do .NET 9):**
   * Decisão: Utilizar `Guid.CreateVersion7()` como valor padrão para todas as chaves primárias.
   * Motivo: Combina um prefixo temporal sequencial com entropia aleatória. Isso resolve 100% o problema clássico de fragmentação de índice B-Tree no SQL Server (mantendo performance equivalente a inteiros) e simultaneamente previne ataques de enumeração/IDOR e vazamento de métricas corporativas, além de permitir geração de ID em memória no cliente antes de persistir no banco.
2. **Separação de Responsabilidades e Sanitização:**
   * Domínio: POCOs puros com encapsulamento DDD.
   * Banco de dados: Colunas limpas sanitizadas (`VARCHAR(14)` para CNPJ e `VARCHAR(11)` para Telefone sem caracteres de pontuação).
   * Relacionamentos: Configurados unicamente no lado dependente para evitar redundâncias no grafo do EF Core.
3. **Imutabilidade Financeira e Rastreadores:**
   * As faturas registram competência e valor total fixados no momento da cobrança para garantir rastreabilidade contábil imutável.
   * Relatórios contábeis e fiscais devem explicitamente invocar `.IgnoreQueryFilters()` para manter a visibilidade sobre empresas e planos inativos em competências passadas.
4. **Política de Falha Rápida no Startup (Fail-Fast):**
   * Se o banco ou o seed falharem no início da aplicação em desenvolvimento, o processo é abortado imediatamente (`throw`), prevenindo que o sistema opere em estado degradado ou inconsistente.
