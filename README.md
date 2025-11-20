# 🎓 EducationCenter API – Plataforma Educacional

A **EducationCenter API** é uma API RESTful desenvolvida em **.NET 8**, voltada para o gerenciamento de uma plataforma educacional.  
Ela fornece acesso a recursos como:

- **Profissões** (ex.: Back-end, Front-end, Dados)
- **Vídeos** educacionais
- **Trilhas de aprendizagem (Learning Paths)**
- **Alunos**

A API segue boas práticas de engenharia de software:

- ✔️ Paginação
- ✔️ HATEOAS
- ✔️ Versionamento (API v1)
- ✔️ Status codes adequados
- ✔️ Logging estruturado
- ✔️ Health Checks (liveness e readiness)
- ✔️ Tracing com OpenTelemetry

---

## 🏗️ Arquitetura da Aplicação

/EducationCenter
├── Controllers/
│ ├── V1/
│ │ ├── StudentsController.cs
│ │ ├── VideosController.cs
│ │ ├── ProfessionsController.cs
│ │ ├── LearningPathsController.cs
├── DTOs/
├── Models/
├── Data/
│ ├── EducationalCenterContext.cs
├── Program.cs


---

## 📁 Recursos Disponíveis

### 👨‍🎓 Students
Gerencia alunos cadastrados.

Endpoints:
- `GET /api/v1/students`
- `GET /api/v1/students/{id}`
- `POST /api/v1/students`
- `PUT /api/v1/students/{id}`
- `DELETE /api/v1/students/{id}`

---

### 🎥 Videos
Gerencia vídeos disponíveis para estudo.

Endpoints:
- `GET /api/v1/videos`
- `GET /api/v1/videos/{id}`
- `POST /api/v1/videos`
- `PUT /api/v1/videos/{id}`
- `DELETE /api/v1/videos/{id}`

---

### 📚 Learning Paths
Agrupa vídeos em trilhas de aprendizagem.

Endpoints:
- `GET /api/v1/learningpaths`
- `GET /api/v1/learningpaths/{id}`
- `POST /api/v1/learningpaths`
- `PUT /api/v1/learningpaths/{id}`
- `DELETE /api/v1/learningpaths/{id}`

---

### 💼 Professions
Lista profissões relacionadas ao mercado de tecnologia.

Endpoints:
- `GET /api/v1/professions`
- `GET /api/v1/professions/{id}`
- `POST /api/v1/professions`
- `PUT /api/v1/professions/{id}`
- `DELETE /api/v1/professions/{id}`

---

## 🔍 Paginação

Todos os endpoints de listagem implementam paginação:
GET /api/v1/students?pageNumber=1&pageSize=10

Retorno:

```json
{
  "items": [...],
  "pageNumber": 1,
  "pageSize": 10,
  "totalItems": 42,
  "totalPages": 5,
  "links": [...]
}

A API segue o estilo HATEOAS, retornando links úteis nos recursos:
{
  "data": { ... },
  "links": [
    { "href": "/api/v1/students/1", "rel": "self", "method": "GET" },
    { "href": "/api/v1/students", "rel": "students", "method": "GET" },
    { "href": "/api/v1/students/1", "rel": "update_student", "method": "PUT" },
    { "href": "/api/v1/students/1", "rel": "delete_student", "method": "DELETE" }
  ]
}
📡 Health Checks

Fornecidos em:

GET /health/live

GET /health/ready

Exemplo de /health/ready:
{
  "status": "Healthy",
  "checks": [
    {
      "name": "database",
      "status": "Healthy",
      "description": null
    }
  ]
}

📝 Logging

O projeto usa:

ILogger<T>

Logs de Informação, Aviso e Erro

Correlação via HttpContext.TraceIdentifier

🛰️ Tracing

Instrumentação com OpenTelemetry:

Requests HTTP

HttpClient

🚀 Como executar
1. Instalar dependências
dotnet restore

2. Rodar o projeto
dotnet run --project ./EducationCenter/EducationCenter.csproj

3. Acessar o Swagger
https://localhost:{porta}/swagger


✔️ A documentação da API estará lá.

📌 Versionamento

A API atual usa v1:

/api/v1/students
/api/v1/videos
/api/v1/learningpaths
/api/v1/professions


Pro futuro, novas versões serão adicionadas em:

/api/v2/*


mantendo compatibilidade com clientes antigos.

✨ Tecnologias Utilizadas

.NET 8

Entity Framework Core

OpenTelemetry

Swagger

HealthChecks

HATEOAS

InMemory Database (modo demo)

Exportação para Console

