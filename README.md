# FCG Users API

Microsserviço responsável por **autenticação, registro e gestão de usuários** da plataforma FCG (Full Cycle Gaming).

## Visão Geral

| | |
|---|---|
| **Runtime** | .NET 8 — Minimal API |
| **Porta local** | `8081` (Docker: `8081:8080`) |
| **Banco de dados** | PostgreSQL 16 |
| **Autenticação** | JWT Bearer |
| **Mensageria** | RabbitMQ + MassTransit |
| **Swagger** | http://localhost:8081/swagger |

## Estrutura do Projeto

```
fcg-users-api/
├── src/
│   ├── FCG.UsersAPI.Domain/          # Entidades, interfaces, value objects
│   ├── FCG.UsersAPI.Application/     # Commands, queries, handlers, DTOs
│   ├── FCG.UsersAPI.Infrastructure/  # EF Core, JWT, RabbitMQ, repositórios
│   └── FCG.UsersAPI.API/             # Endpoints Minimal API, middlewares, Program.cs
├── tests/
│   ├── FCG.UsersAPI.Tests/           # Testes unitários (xUnit)
│   └── FCG.UsersAPI.IntegrationTests/# Testes de integração
└── Dockerfile
```

## Endpoints

| Método | Rota | Autenticação | Descrição |
|--------|------|:---:|---|
| `POST` | `/auth/registro` | ❌ | Cadastro de novo usuário |
| `POST` | `/auth/login` | ❌ | Login — retorna JWT |
| `GET` | `/usuarios/perfil` | ✅ | Perfil do usuário autenticado |
| `PUT` | `/usuarios/perfil` | ✅ | Atualiza dados do perfil |
| `GET` | `/health` | ❌ | Health check |
| `GET` | `/metrics` | ❌ | Métricas Prometheus |

## JWT

```
Issuer:   fcg-users-api
Audience: fcg-platform
Secret:   fcg-super-secret-key-change-in-production-min32chars!
Expiração: 8 horas
```

## Eventos Publicados (RabbitMQ)

| Evento | Quando |
|--------|--------|
| `UsuarioCadastradoEvent` | Novo usuário registrado |

## Variáveis de Ambiente

```env
ConnectionStrings__Postgres=Host=postgres;Database=fcg;Username=fcg_app;Password=fcg_app_dev
Jwt__Secret=fcg-super-secret-key-change-in-production-min32chars!
Jwt__Issuer=fcg-users-api
Jwt__Audience=fcg-platform
RabbitMQ__Host=rabbitmq
RabbitMQ__VirtualHost=/
RabbitMQ__Username=guest
RabbitMQ__Password=guest
SeedAdmin__Email=admin@fcg.com
SeedAdmin__Senha=TrocarEm@2026
```

## Executar Localmente

```bash
# Via Docker Compose (recomendado)
cd ../fcg-orchestration
docker compose up -d users-api

# Via dotnet
cd src/FCG.UsersAPI.API
dotnet run
```

## Executar Testes

```bash
dotnet test
```

## Observabilidade

- **Health check:** `GET /health`
- **Métricas Prometheus:** `GET /metrics` (scraped pelo Prometheus a cada 15s)
- **Biblioteca:** `prometheus-net.AspNetCore 8.2.1`

---

> **FIAP Pós-Tech — Software Architecture | Tech Challenge**
Lucas Monte Ferreri Castilho
