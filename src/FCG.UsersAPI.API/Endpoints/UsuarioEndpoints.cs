using FluentValidation;
using FCG.UsersAPI.API.Comum;
using FCG.UsersAPI.Application.Identidade.DTOs;
using FCG.UsersAPI.Application.Identidade.Servicos;
using FCG.UsersAPI.Domain.Identidade.Enums;

namespace FCG.UsersAPI.API.Endpoints;

public static class UsuarioEndpoints
{
    public static void MapUsuarioEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/usuarios").WithTags("Usuários");

        g.MapGet("/", async (Guid? id, string? role, int? pagina, int? tamanhoPagina,
            ListarUsuariosService svc, CancellationToken ct) =>
        {
            TipoUsuario? filtroTipo = null;
            if (role is not null)
            {
                if (!Enum.TryParse<TipoUsuario>(role, ignoreCase: true, out var tipo))
                    return Results.BadRequest(new ErroResponse($"Role inválida. Valores aceitos: {string.Join(", ", Enum.GetNames<TipoUsuario>())}"));
                filtroTipo = tipo;
            }
            var r = await svc.ExecutarAsync(id, filtroTipo, pagina ?? 1, tamanhoPagina ?? 20, ct);
            return r.ToHttpResult(v => Results.Ok(v));
        })
        .RequireAuthorization("AdminOnly")
        .WithName("ListarUsuarios")
        .WithSummary("Listar usuários")
        .WithDescription(
            "Retorna a lista paginada de usuários. Filtros opcionais: `id` (GUID), `role` (Usuario | Administrador), `pagina`, `tamanhoPagina`.\n\n" +
            "**400** – valor de `role` não reconhecido.\n\n" +
            "**401** – token ausente ou inválido.\n\n" +
            "**403** – usuário autenticado não tem perfil Administrador.")
        .Produces<ListarUsuariosResponse>(200)
        .Produces<ErroResponse>(400)
        .Produces<ErroResponse>(401)
        .Produces<ErroResponse>(403);

        g.MapGet("/{id:guid}", async (Guid id, ListarUsuariosService svc, CancellationToken ct) =>
        {
            var r = await svc.ExecutarAsync(id, null, 1, 1, ct);
            return r.ToHttpResult(v => v.Itens.Count > 0
                ? Results.Ok(v.Itens[0])
                : Results.NotFound(new ErroResponse("Usuário não encontrado.")));
        })
        .RequireAuthorization("AdminOnly")
        .WithName("BuscarUsuarioPorId")
        .WithSummary("Buscar usuário por ID")
        .WithDescription(
            "Retorna os dados de um usuário específico pelo seu GUID. Requer perfil Administrador.\n\n" +
            "**401** – token ausente ou inválido.\n\n" +
            "**403** – usuário autenticado não tem perfil Administrador.\n\n" +
            "**404** – nenhum usuário encontrado com o ID informado.")
        .Produces<UsuarioResponse>(200)
        .Produces<ErroResponse>(401)
        .Produces<ErroResponse>(403)
        .Produces<ErroResponse>(404);

        g.MapPost("/registrar", async (RegistrarUsuarioRequest req, IValidator<RegistrarUsuarioRequest> v,
            CadastrarUsuarioService svc, CancellationToken ct) =>
        {
            var falha = await v.ValidarOuFalharAsync(req, ct);
            if (falha is not null) return falha;
            var r = await svc.ExecutarAsync(req, ct);
            return r.ToHttpResult(val => Results.Created($"/api/usuarios/{val.Id}", val));
        })
        .WithName("RegistrarUsuario")
        .WithSummary("Registrar novo usuário")
        .WithDescription(
            "Cria uma nova conta com perfil padrão (Usuario). Não requer autenticação.\n\n" +
            "**400** – nome, e-mail ou senha não atendem às regras de validação.\n\n" +
            "**409** – já existe uma conta com o e-mail informado.")
        .Produces<UsuarioResponse>(201)
        .Produces<ValidacaoErroResponse>(400)
        .Produces<ErroResponse>(409);

        g.MapPost("/administradores", async (RegistrarUsuarioRequest req, IValidator<RegistrarUsuarioRequest> v,
            CadastrarUsuarioService svc, CancellationToken ct) =>
        {
            var falha = await v.ValidarOuFalharAsync(req, ct);
            if (falha is not null) return falha;
            var r = await svc.ExecutarComoAdminAsync(req, ct);
            return r.ToHttpResult(val => Results.Created($"/api/usuarios/{val.Id}", val));
        })
        .RequireAuthorization("AdminOnly")
        .WithName("RegistrarAdministrador")
        .WithSummary("Registrar novo administrador")
        .WithDescription(
            "Cria uma nova conta com perfil Administrador. Requer que o solicitante já seja Administrador.\n\n" +
            "**400** – dados de entrada inválidos.\n\n" +
            "**401** – token ausente ou inválido.\n\n" +
            "**403** – usuário autenticado não tem perfil Administrador.\n\n" +
            "**409** – já existe uma conta com o e-mail informado.")
        .Produces<UsuarioResponse>(201)
        .Produces<ValidacaoErroResponse>(400)
        .Produces<ErroResponse>(401)
        .Produces<ErroResponse>(403)
        .Produces<ErroResponse>(409);

        g.MapPut("/{id:guid}", async (Guid id, AtualizarUsuarioRequest req,
            AtualizarUsuarioService svc, CancellationToken ct) =>
        {
            var r = await svc.ExecutarAsync(id, req, ct);
            return r.ToHttpResult(val => Results.Ok(val));
        })
        .RequireAuthorization("AdminOnly")
        .WithName("AtualizarUsuario")
        .WithSummary("Atualizar dados do usuário")
        .WithDescription(
            "Atualiza o nome do usuário identificado pelo ID. Requer perfil Administrador.\n\n" +
            "**400** – nome vazio ou inválido.\n\n" +
            "**401** – token ausente ou inválido.\n\n" +
            "**403** – usuário autenticado não tem perfil Administrador.\n\n" +
            "**404** – nenhum usuário encontrado com o ID informado.")
        .Produces<UsuarioResponse>(200)
        .Produces<ErroResponse>(400)
        .Produces<ErroResponse>(401)
        .Produces<ErroResponse>(403)
        .Produces<ErroResponse>(404);

        g.MapPatch("/{id:guid}/promover", async (Guid id,
            PromoverUsuarioService svc, CancellationToken ct) =>
        {
            var r = await svc.ExecutarAsync(id, ct);
            return r.ToHttpResult(v => Results.Ok(v));
        })
        .RequireAuthorization("AdminOnly")
        .WithName("PromoverUsuario")
        .WithSummary("Promover usuário a Administrador")
        .WithDescription(
            "Altera o perfil do usuário de Usuario para Administrador. Operação irreversível. Requer perfil Administrador.\n\n" +
            "**401** – token ausente ou inválido.\n\n" +
            "**403** – usuário autenticado não tem perfil Administrador.\n\n" +
            "**404** – nenhum usuário encontrado com o ID informado.\n\n" +
            "**409** – o usuário já possui perfil Administrador.")
        .Produces<UsuarioResponse>(200)
        .Produces<ErroResponse>(401)
        .Produces<ErroResponse>(403)
        .Produces<ErroResponse>(404)
        .Produces<ErroResponse>(409);

        g.MapDelete("/{id:guid}", async (Guid id,
            RemoverUsuarioService svc, CancellationToken ct) =>
        {
            var r = await svc.ExecutarAsync(id, ct);
            return r.ToHttpResult();
        })
        .RequireAuthorization("AdminOnly")
        .WithName("RemoverUsuario")
        .WithSummary("Remover usuário")
        .WithDescription(
            "Remove permanentemente o usuário da plataforma. Requer perfil Administrador. Atenção: operação irreversível.\n\n" +
            "**401** – token ausente ou inválido.\n\n" +
            "**403** – usuário autenticado não tem perfil Administrador.\n\n" +
            "**404** – nenhum usuário encontrado com o ID informado.")
        .Produces(204)
        .Produces<ErroResponse>(401)
        .Produces<ErroResponse>(403)
        .Produces<ErroResponse>(404);
    }
}
