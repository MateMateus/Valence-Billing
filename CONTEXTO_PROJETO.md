# Contexto do Projeto: Motor de Faturamento Recorrente B2B (Billing Engine)

> **Documento Vivo de Arquitetura e Contexto**  
> Este arquivo reúne todo o escopo, arquitetura, decisões técnicas, regras de trabalho estritas e o progresso atual do projeto. Qualquer IA ou desenvolvedor deve ler este documento antes de sugerir ou continuar o desenvolvimento.

---

## 1. Visão Geral e Objetivo do Sistema

* **Projeto:** Motor de Faturamento Recorrente e Cobrança B2B (SaaS / Fintech).
* **Solução:** `ValenceBilling.sln`
* **Objetivo:** Sistema de alta complexidade corporativa para gerenciar planos de assinatura, clientes corporativos (B2B), contratos de recorrência, emissão automatizada de faturas e controle de inadimplência/pagamentos.
* **Nível:** Projetado com padrões e exigências de processos seletivos plenos/seniores (concorrência, integridade referencial, imutabilidade financeira, auditoria, DDD e Clean Architecture).

---

## 2. Stack Tecnológica e Ferramentas

| Tecnologia | Função no Sistema | Status |
| :--- | :--- | :--- |
| **.NET 9 (C#)** | Plataforma principal da API e Web | Configurado (`global.json` e `.csproj`) |
| **Entity Framework Core 9** | ORM, Migrations e Fluent API | Instalado e configurado |
| **SQL Server** | Banco de dados relacional | LocalDB configurado e integrado |
| **ASP.NET Core Identity** | Autenticação, autorização e perfis | Configurado com chaves `Guid` |
| **Bootstrap 5.3** | Interface web responsiva | Instalado no `ValenceBilling.Web/wwwroot` |
| **JavaScript (Fetch API)** | Comunicação assíncrona com a API | Nativo do navegador |
| **Chart.js** | Gráficos do dashboard financeiro | Será importado via CDN nas telas Razor |

---

## 3. Arquitetura e Divisão das Camadas (Clean Architecture)

A solução segue a separação estrita de responsabilidades:

1. **`ValenceBilling.Domain` (Coração do Negócio) - [CONCLUÍDO]:**
   * Totalmente puro e agnóstico de frameworks/bancos de dados.
   * Contém as entidades, enums, regras de negócio e invariantes (DDD: métodos explícitos de transição de estado como `Ativar()` e `Desativar()`).
   * **Sem DataAnnotations** (o mapeamento de banco é feito exclusivamente via Fluent API na infraestrutura).

2. **`ValenceBilling.Infrastructure` (Acesso a Dados e Persistência) - [CONCLUÍDO]:**
   * Acesso a dados (`AppDbContext`), mapeamentos Fluent API, Identity (`ApplicationUser`), Migrations e Seeds idempotentes e resilientes.

3. **`ValenceBilling.Application` (Casos de Uso e Regras de Aplicação) - [PRÓXIMA FASE]:**
   * Casos de uso / Services (ciclo de assinatura, motor de geração de faturas, baixa de pagamentos), DTOs, interfaces de repositórios/serviços e validações com FluentValidation.

4. **`ValenceBilling.Api` (Exposição RESTful e Startup):**
   * Endpoints RESTful para integração externa, processamento assíncrono, OpenAPI/Swagger e pipeline de execução automática de migrations e seed em desenvolvimento.

5. **`ValenceBilling.Web` (Apresentação / Backoffice):**
   * Aplicação MVC (Razor Views + Controllers) para o painel administrativo/backoffice e dashboards financeiros.

---

## 4. Regras de Trabalho Mandatórias (Instruções Estritas para a IA)

> [!IMPORTANT]
> 1. **A IA NUNCA adiciona ou altera código C# direto no disco sozinha:**
>    * O desenvolvedor é quem aplica todo o código no projeto.
>    * A IA atua exclusivamente como **arquiteta de software, mentora técnica e revisora**.
>    * A IA deve **sempre apresentar o modelo de código completo e didático no chat** para o desenvolvedor copiar/digitar no arquivo correspondente.
> 2. **Validação, Compilação e Commit pela IA:**
>    * Assim que o desenvolvedor avisa que colou/digitou o código no arquivo, a IA valida o arquivo, roda a compilação (`dotnet build`) para garantir zero erros e **executa o commit atômico correspondente**.
> 3. **Estratégia de Branching Obrigatória (GitHub Flow / Feature Branches):**
>    * **NUNCA commitar novidades diretamente na branch `main`**.
>    * A cada nova fase, módulo ou conjunto de funcionalidades, **deve-se criar uma nova branch** a partir da `main` (ex.: `feature/infra-persistence`, `feature/application-billing`).
>    * Todos os commits atômicos daquele módulo são feitos na branch específica.
>    * Apenas após a conclusão, validação e autorização do usuário, a branch é mergeada na `main` utilizando obrigatoriamente **`git merge --no-ff`** para preservar a linha curva e o gráfico histórico visual no Git Graph.
> 4. **Atualização Contínua do Contexto:**
>    * Ao concluir marcos ou decisões importantes, a IA deve orientar ou registrar as alterações neste arquivo `CONTEXTO_PROJETO.md`.

---

## 5. Modelo de Domínio (Agregados Principais)

1. **`Cliente`:**
   * Empresas B2B tomadoras do serviço.
   * Campos: `Id` (`Guid.CreateVersion7`), `RazaoSocial`, `NomeFantasia`, `Cnpj` (único, 14 dígitos numéricos), `EmailFinanceiro`, `Telefone` (11 dígitos numéricos limpos), `IsActive` (`private set`), `CreatedAt`.
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

## 6. Histórico de Fases do Projeto

### **Fase 1: Domínio, Infraestrutura, Persistência e API (CONCLUÍDA):**
* **Branch utilizada:** `feature/infra-persistence` (mergeada na `main` com `--no-ff`).
* **Entregáveis Concluídos:**
  * Domínio puro modelado com UUIDv7 (`Guid.CreateVersion7`) e métodos DDD.
  * Mapeamentos Fluent API limpos (relacionamentos centralizados unicamente no lado dependente).
  * `ApplicationUser` integrado ao ASP.NET Identity com chave `Guid` e FK opcional para `Cliente` (`DeleteBehavior.SetNull`).
  * Migration `InitialCreate` gerada com sucesso via EF Core CLI.
  * `DatabaseSeeder` 100% idempotente e resiliente:
    * Uso de `.IgnoreQueryFilters()` para não colidir com soft delete.
    * Inserção plano a plano tolerante a estados parciais.
    * CNPJ de teste matematicamente válido (`45997418000153`).
    * Reativação automática de clientes/planos inativos no ambiente de desenvolvimento.
    * Cálculo de vencimento atômico e consistente a partir de captura única de `DateTime.UtcNow`.
  * API integrada (`Program.cs`) com aplicação automática de migrations e seed em desenvolvimento, com política *Fail-Fast* (`throw` em caso de erro crítico no startup).

### **Fase 2: Camada de Aplicação (Próxima Etapa - A Fazer):**

> **Comando inicial obrigatório para iniciar esta fase:**  
> `git checkout -b feature/application-services`

#### **Roteiro Direto de Implementação:**

1. **Pacotes:**
   * Adicionar `FluentValidation.DependencyInjectionExtensions` no `ValenceBilling.Application`.
2. **DTOs e Validações de Entrada (FluentValidation):**
   * `ClienteDTO` + `ClienteValidator` (validação matemática dos dígitos verificadores do CNPJ e e-mail).
   * `PlanoDTO` + `PlanoValidator` (preço mensal > 0 e nome obrigatório).
   * `AssinaturaDTO` + `AssinaturaValidator` (dia de vencimento entre 1 e 31, IDs válidos).
   * `FaturaDTO` (dados para exibição e pagamento).
3. **Casos de Uso / Serviços de Aplicação (`Services/`):**
   * **`IClienteService` / `ClienteService`:** Cadastro e inativação de clientes.
   * **`IAssinaturaService` / `AssinaturaService`:** Criação, renovação e cancelamento de contratos.
   * **`IFaturamentoEngine` / `FaturamentoEngine` (Motor Recorrente):** Varrer assinaturas ativas com período vencendo e gerar as faturas do mês na competência (`yyyy-MM`) de forma idempotente (sem duplicidade).
   * **`IPagamentoService` / `PagamentoService` (Liquidação):** Baixar faturas pagas e reativar automaticamente assinaturas suspensas por inadimplência.
4. **Injeção de Dependência:**
   * Criar `DependencyInjection.cs` na Application expondo o método de extensão `services.AddApplication()` para registrar serviços e validadores.

---

## 7. Decisões Arquiteturais Registradas

1. **Adoção do UUIDv7 (`Guid.CreateVersion7()` do .NET 9):**
   * Padrão para todas as PKs: combina timestamp sequencial (alta performance no índice B-Tree) com segurança contra enumeração/IDOR e geração de chave pré-banco.
2. **Sanitização de Dados:**
   * `Cnpj`: 14 dígitos numéricos puros no banco.
   * `Telefone`: 11 dígitos numéricos puros no banco (DDD + número). Máscaras e formatações pertencem exclusivamente à camada de apresentação/UI.
3. **Mapeamento Unidirecional de Dependência no EF Core:**
   * Relações 1:N são configuradas apenas na entidade dependente (a que possui a FK), eliminando declarações duplicadas e potenciais conflitos no grafo do EF Core.
4. **Imutabilidade Financeira e Visibilidade Histórica:**
   * Faturas preservam o valor e competência da data de emissão.
   * Consultas contábeis e fiscais devem aplicar obrigatoriamente `.IgnoreQueryFilters()` para não omitir dados de empresas ou planos desativados via soft delete.
5. **Fail-Fast no Startup:**
   * A API encerra imediatamente com `throw` se a migration ou o seed falharem em ambiente de desenvolvimento, evitando estados zumbis.
6. **Git Branching Visual (No-Fast-Forward):**
   * Toda feature é isolada em sua própria branch e integrada via `git merge --no-ff`, garantindo rastreabilidade no gráfico e histórico profissional.
