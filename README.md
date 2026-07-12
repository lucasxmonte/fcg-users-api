# FCG Users API — Microsserviço de Usuários

Microsserviço responsável por **cadastro, autenticação (JWT) e autorização** de usuários da plataforma FIAP Cloud Games (FCG).

## Responsabilidades

- Registro de usuários comuns e administradores
- Autenticação via JWT (access token 15min + refresh token rotativo 7 dias)
- Listagem e promoção de usuários (admin)
- Publicação de `UserCreatedEvent` via RabbitMQ (consumido pelo NotificationsAPI)

## Stack

| Componente | Tecnologia |
|---|---|
| Framework | .NET 8 Minimal API |
| ORM | EF Core 8 + Npgsql (PostgreSQL) |
| Mensageria | MassTransit + RabbitMQ |
| Auth | JWT (HMAC SHA256) + Refresh Token |
| Hash | BCrypt (WorkFactor 12) |
| Docs | Swagger / OpenAPI |

## Endpoints

| Método | Rota | Auth | Descrição |
|---|---|---|---|
| POST | `/api/auth/login` | público | Autentica e retorna tokens |
| POST | `/api/auth/refresh` | público | Renova access token |
| POST | `/api/auth/logout` | autenticado | Invalida refresh token |
| GET | `/api/usuarios` | admin | Lista usuários |
| POST | `/api/usuarios/registrar` | público | Registra usuário comum |
| POST | `/api/usuarios/administradores` | admin | Cria administrador |
| PATCH | `/api/usuarios/{id}/promover` | admin | Promove para admin |
| GET | `/health` | público | Health check |

## Variáveis de Ambiente

| Variável | Descrição | Padrão |
|---|---|---|
| `ConnectionStrings__Postgres` | Connection string do PostgreSQL | — |
| `Jwt__Secret` | Secret HMAC SHA256 (mín. 32 chars) | — |
| `Jwt__Issuer` | Issuer do token | `fcg-users-api` |
| `Jwt__Audience` | Audience do token | `fcg-platform` |
| `RabbitMQ__Host` | Host do RabbitMQ | `localhost` |
| `RabbitMQ__Username` | Usuário RabbitMQ | `guest` |
| `RabbitMQ__Password` | Senha RabbitMQ | `guest` |
| `SeedAdmin__Email` | E-mail do admin inicial | `admin@fcg.com` |
| `SeedAdmin__Senha` | Senha do admin inicial | `TrocarEm@2026` |

## Executar com Docker Compose

```bash
docker compose up -d --build
```

API disponível em: `http://localhost:8081`
Swagger: `http://localhost:8081/swagger`

## Executar localmente

```bash
# Pré-requisitos: .NET 8 SDK + PostgreSQL + RabbitMQ

dotnet restore
dotnet ef database update -p src/FCG.UsersAPI.Infrastructure -s src/FCG.UsersAPI.API
dotnet run --project src/FCG.UsersAPI.API
```

## Gerar Migrations

```bash
dotnet ef migrations add Initial \
  -p src/FCG.UsersAPI.Infrastructure \
  -s src/FCG.UsersAPI.API \
  -o Persistencia/Migrations
```

## Deploy Kubernetes

```bash
kubectl apply -f k8s/namespace.yaml
kubectl apply -f k8s/
kubectl get pods -n fcg
```

## Evento publicado

```json
// UserCreatedEvent (publicado após cada registro bem-sucedido)
{
  "userId": "guid",
  "nome": "João Silva",
  "email": "joao@email.com",
  "dataCadastro": "2026-01-01T00:00:00Z"
}
```

## Grupo 17 — Pos-Tech FIAP
