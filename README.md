Membros
Oscar Arias Neto	556936
Nicolas Souza dos Santos	555571
Julia Martis Rebelles	554516

# EducationCenter API

API RESTful em .NET 8 para gerenciamento de trilhas de aprendizagem, profissões, alunos e vídeos, com:

- **.NET 8 Web API**
- **Entity Framework Core** + **Oracle** (tabelas `EC_*`)
- **Versionamento de API** (v1 e v2)
- **Swagger/OpenAPI 3.0** com dropdown de versões
- **Health Checks**
- **OpenTelemetry** (tracing via console)
- Integração simples com **Google Gemini API** (endpoint `/gemini/ask`)
- Testes automatizados com **xUnit** (`EducationCenter.Tests`)

Repositório: `https://github.com/OscarAriasNeto/EducationCenterApi` (privado).

---

## Sumário

- [Arquitetura e projetos](#arquitetura-e-projetos)
- [Tecnologias e bibliotecas](#tecnologias-e-bibliotecas)
- [Pré-requisitos](#pré-requisitos)
- [Configuração do banco Oracle](#configuração-do-banco-oracle)
- [Configuração de appsettings (Oracle + Gemini)](#configuração-de-appsettings-oracle--gemini)
- [Como executar a API](#como-executar-a-api)
- [Como executar os testes](#como-executar-os-testes)
- [Swagger e versionamento de API](#swagger-e-versionamento-de-api)
- [Endpoints principais](#endpoints-principais)
- [Health checks](#health-checks)
- [Estrutura de pastas](#estrutura-de-pastas)
- [Notas de segurança (senhas, API keys)](#notas-de-segurança-senhas-api-keys)

---

## Arquitetura e projetos

A solução contém dois projetos principais:

- `EducationCenter`  
  Projeto Web API (.NET 8) com controllers v1 e v2, integração com Oracle, Gemini, health checks e tracing.

- `EducationCenter.Tests`  
  Projeto de testes unitários com **xUnit**, usando **EF Core InMemory** para isolar os testes do banco Oracle real.

---

## Tecnologias e bibliotecas

- **Linguagem / Runtime**
  - .NET 8
  - C# 12

- **Web / API**
  - ASP.NET Core Web API
  - Versionamento: `Microsoft.AspNetCore.Mvc.Versioning`
  - Swagger/OpenAPI: `Swashbuckle.AspNetCore`

- **Banco de dados**
  - Oracle Database (cloud da FIAP)
  - EF Core provider: `Oracle.EntityFrameworkCore`
  - Mapeamento para tabelas:
    - `EC_PROFESSION`
    - `EC_LEARNING_PATH`
    - `EC_LEARNING_PATH_VIDEO`
    - `EC_STUDENT`
    - `EC_STUDENT_LEARNING_PATH`
    - `EC_VIDEO`
    - `EC_AUDIT`

- **Observabilidade**
  - OpenTelemetry:
    - `OpenTelemetry.Extensions.Hosting`
    - `OpenTelemetry.Instrumentation.AspNetCore`
    - `OpenTelemetry.Instrumentation.Http`
    - `OpenTelemetry.Exporter.Console`

- **Testes**
  - `xunit`
  - `Moq`
  - `Microsoft.NET.Test.Sdk`

---

## Pré-requisitos

Antes de rodar o projeto, instale:

1. **.NET 8 SDK**  
   https://dotnet.microsoft.com/en-us/download/dotnet/8.0

2. **Cliente Oracle / acesso ao banco FIAP**  
   - Acesso à instância Oracle FIAP:  
     - Host: `oracle.fiap.com.br`  
     - Porta: `1521`  
     - SID/Service: `orcl`
   - Um usuário de banco criado para o projeto (por exemplo `RM123456`).

3. **Ferramenta de SQL para Oracle**  
   - Ex.: SQL Developer, DBeaver, DataGrip ou outro cliente similar.

4. (Opcional) **Postman / Insomnia** para testes de API, além do Swagger.

---

## Configuração do banco Oracle

1. **Criar usuário (se ainda não existir)**  

   No ambiente da FIAP, o usuário normalmente já é fornecido pelo professor (ex.: `RMxxxxx`).  
   Caso o script de criação seja necessário, ele fica a cargo do ambiente da FIAP.

2. **Criar as tabelas do projeto**

   No repositório há um script SQL:

   - `EducationCenter.sql`

   Execute o conteúdo desse arquivo conectado ao seu usuário Oracle (ex.: `RMXXXXX`) para criar as tabelas:

   - `EC_PROFESSION`
   - `EC_LEARNING_PATH`
   - `EC_LEARNING_PATH_VIDEO`
   - `EC_STUDENT`
   - `EC_STUDENT_LEARNING_PATH`
   - `EC_VIDEO`
   - `EC_AUDIT`

3. **Dados de acesso (exemplo para DEV)**

> ⚠️ **IMPORTANTE:**  
> Não deixe usuário/senha reais expostos em repositórios públicos.  
> Abaixo usamos valores **de exemplo**, substitua pelos seus dados reais.

- Usuário: `RMXXXXX`  
- Senha: `SUA_SENHA_AQUI`  
- Connection string (Oracle):  

  ```text
  User Id=RMXXXXX;Password=SUA_SENHA_AQUI;Data Source=oracle.fiap.com.br:1521/orcl;
  ```

---

## Configuração de appsettings (Oracle + Gemini)

No projeto `EducationCenter`, configure **`appsettings.Development.json`** (ou `appsettings.json`) com as seções de conexão Oracle e Gemini:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },

  "ConnectionStrings": {
    "OracleDb": "User Id=RMXXXXX;Password=SUA_SENHA_AQUI;Data Source=oracle.fiap.com.br:1521/orcl;"
  },

  "Gemini": {
    "ApiKey": "SUA_API_KEY_DO_GEMINI",
    "Model": "gemini-2.5-flash"
  },

  "AllowedHosts": "*"
}
```

### Sobre a API Key do Gemini

- Crie um projeto no **Google AI Studio / Google Cloud** e gere uma API key para o Gemini.
- Preencha em `"Gemini:ApiKey"`.
- Modelo recomendado: `"gemini-2.5-flash"` (ou `gemini-2.5-flash-lite`).

> Sugestão de boas práticas:
> - Em vez de deixar credenciais fixas no `appsettings.json` da solução, pode usar:
>   - **User Secrets** (`dotnet user-secrets`)
>   - Variáveis de ambiente
> - Como este repositório é privado para avaliação, os valores podem ser mantidos neste arquivo, mas **não é indicado** se o repositório for tornar-se público.

---

## Como executar a API

1. **Clonar o repositório**

```bash
git clone https://github.com/OscarAriasNeto/EducationCenterApi.git
cd EducationCenterApi
```

2. **Conferir/ajustar `appsettings.Development.json`**

   - Verifique a seção `ConnectionStrings:OracleDb`.
   - Verifique a seção `Gemini`.

3. **Restaurar dependências e compilar**

```bash
cd EducationCenter
dotnet restore
dotnet build
```

4. **Executar a aplicação**

```bash
dotnet run
```

Por padrão, a API sobe nas portas configuradas em `launchSettings.json`, por exemplo:

- HTTPS: `https://localhost:5047`
- HTTP:  `http://localhost:5048` (valores podem variar)

5. **Acessar o Swagger**

Abra no navegador:

- https://localhost:5047/swagger

A UI do Swagger exibirá um **dropdown no canto superior direito** com as versões:

- `EducationCenter 1.0`
- `EducationCenter 2.0` (se configurada)

---

## Como executar os testes

Os testes estão no projeto `EducationCenter.Tests` e usam EF Core InMemory (não precisam de banco Oracle real).

1. Na raiz da solução:

```bash
dotnet test
```

ou, se quiser rodar apenas os testes desse projeto:

```bash
cd EducationCenter.Tests
dotnet test
```

2. Os testes cobrem principalmente:

- Controllers de **LearningPaths**, **Professions**, **Students**, **Videos** (v1 e v2)
- Uso de **PagedResponse** e **Resource DTOs**
- Comportamentos de **GET by id**, **GET paginado**, etc.

---

## Swagger e versionamento de API

O projeto utiliza o pacote `Microsoft.AspNetCore.Mvc.Versioning` e um `ApiVersionDescriptionProvider` para gerar um **Swagger por versão**.

- **v1**:
  - Controllers no namespace `EducationCenter.Controllers.v1`
  - Decorados com `[ApiVersion("1.0")]`
  - Rotas do tipo: `/api/v{version:apiVersion}/learningpaths`

- **v2**:
  - Controllers no namespace `EducationCenter.Controllers.v2`
  - Decorados com `[ApiVersion("2.0")]`
  - Rotas do tipo: `/api/v{version:apiVersion}/learningpaths` (mesmo path, versão diferente)

No Swagger:

- Dropdown “**Select a definition**” permite escolher:
  - `EducationCenter 1.0` → endpoints v1
  - `EducationCenter 2.0` → endpoints v2

---

## Endpoints principais

Abaixo um resumo dos principais endpoints (v1 e v2). Os paths reais dependem da `version` informada na rota (`v1`, `v2`).

### 1. Gemini (v1)

- **POST `/api/v1/gemini/ask`**  
  Faz uma requisição simples ao modelo Gemini configurado.

**Body de exemplo:**

```json
{
  "prompt": "Explique o que é uma API REST."
}
```

**Resposta (exemplo):**

```json
{
  "prompt": "Explique o que é uma API REST.",
  "answer": "Uma API REST ... (texto gerado pelo modelo)."
}
```

---

### 2. LearningPaths

**v1** – In memory (pensado inicialmente para estudo; hoje também mapeado via EF):

- `GET  /api/v1/learningpaths`
- `GET  /api/v1/learningpaths/{id}`
- `POST /api/v1/learningpaths`
- `PUT  /api/v1/learningpaths/{id}`
- `DELETE /api/v1/learningpaths/{id}`

**v2** – Integrado com Oracle (tabelas `EC_LEARNING_PATH`, `EC_LEARNING_PATH_VIDEO`):

- `GET  /api/v2/learningpaths`
- `GET  /api/v2/learningpaths/{id}`
- `POST /api/v2/learningpaths`
- `PUT  /api/v2/learningpaths/{id}`
- `DELETE /api/v2/learningpaths/{id}`

Suporte a paginação:

- Query params `pageNumber` e `pageSize`
- Resposta envelope `PagedResponse<T>` com:
  - `Items`
  - `PageNumber`
  - `PageSize`
  - `TotalItems`
  - `TotalPages`
  - `Links` (HATEOAS)

---

### 3. Professions

**v1**

- `GET  /api/v1/professions`
- `GET  /api/v1/professions/{id}`
- `POST /api/v1/professions`
- `PUT  /api/v1/professions/{id}`
- `DELETE /api/v1/professions/{id}`

**v2** – Usa filtro opcional `search` e paginação:

- `GET  /api/v2/professions?search=dev&pageNumber=1&pageSize=10`
- `GET  /api/v2/professions/{id}`
- `POST /api/v2/professions`
- `PUT  /api/v2/professions/{id}`
- `DELETE /api/v2/professions/{id}`

---

### 4. Students

**v1**

- `GET  /api/v1/students`
- `GET  /api/v1/students/{id}`
- `POST /api/v1/students`
- `PUT  /api/v1/students/{id}`
- `DELETE /api/v1/students/{id}`

**v2** (quando aplicada, similar à v1, mas mapeada às tabelas Oracle `EC_STUDENT` e `EC_STUDENT_LEARNING_PATH`).

---

### 5. Videos

**v1**

- `GET  /api/v1/videos`
- `GET  /api/v1/videos/{id}`
- `GET  /api/v1/videos/by-trail/{learningPathId}`
- `POST /api/v1/videos`
- `PUT  /api/v1/videos/{id}`
- `DELETE /api/v1/videos/{id}`

**v2** (quando aplicada) segue o mesmo padrão, mas usando tabelas `EC_VIDEO` e relacionamento com `EC_LEARNING_PATH_VIDEO`.

---

## Health checks

Dois endpoints básicos de health check são expostos:

- `GET /health/live`  
  Verifica se a API está viva (liveness probe).

- `GET /health/ready`  
  Verifica readiness do aplicativo, incluindo check de `EducationalCenterContext` (banco Oracle).

Útil para monitoramento, Kubernetes ou Application Gateway.

---

## Estrutura de pastas

Visão geral simplificada do projeto `EducationCenter`:

```text
EducationCenter/
  Controllers/
    v1/
      GeminiController.cs
      LearningPathsController.cs
      ProfessionsController.cs
      StudentsController.cs
      VideosController.cs
    v2/
      LearningPathsController.cs
      ProfessionsController.cs
      StudentsController.cs
      VideosController.cs
  Data/
    EducationalCenterContext.cs
  DTOs/
    ... (LearningPath DTOs, Profession DTOs, Student DTOs, Video DTOs, PagedResponse, Resource, LinkDto)
  Models/
    Profession.cs
    LearningPath.cs
    LearningPathVideo.cs
    Student.cs
    StudentLearningPath.cs
    Video.cs
    Audit.cs
  Services/
    GeminiOptions.cs
    GeminiClient.cs
  appsettings.json
  appsettings.Development.json
  Program.cs
  EducationCenter.http        # Arquivo de testes HTTP (VS/VS Code)
```

Projeto de testes `EducationCenter.Tests`:

```text
EducationCenter.Tests/
  LearningPaths/
    LearningPathsControllerTests.cs
    LearningPathsControllerV2Tests.cs
  Professions/
    ProfessionsControllerTests.cs
    ProfessionsControllerV2Tests.cs
  Students/
    StudentsControllerTests.cs
  Videos/
    VideosControllerTests.cs
    VideosControllerV2Tests.cs
  UnitTest1.cs (teste de exemplo)
```

---

## Notas de segurança (senhas, API keys)

- **Não é recomendado** deixar usuário, senha de banco e API keys em texto puro em arquivos versionados, especialmente em repositórios públicos.
- Como este repositório é **privado** para fins de avaliação, os valores podem ser preenchidos diretamente em `appsettings.Development.json`, mas:
  - Evite subir esses arquivos se o repositório for aberto.
  - Considere usar:
    - `dotnet user-secrets`  
      (`dotnet user-secrets set "ConnectionStrings:OracleDb" "...string..."`)
    - Variáveis de ambiente na infraestrutura de deploy.

Se o repositório for compartilhado com professores/avaliadores, você pode:

- Deixar os **placeholders** neste README (como acima)
- Enviar as credenciais reais de DEV por outro canal (Teams, e-mail da turma, etc).

---
