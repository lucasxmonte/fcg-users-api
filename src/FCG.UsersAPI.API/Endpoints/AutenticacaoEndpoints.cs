using FluentValidation;
using FCG.UsersAPI.API.Comum;
using FCG.UsersAPI.Application.Identidade.DTOs;
using FCG.UsersAPI.Application.Identidade.Servicos;

namespace FCG.UsersAPI.API.Endpoints;

public static class AutenticacaoEndpoints
{
    public static void MapAutenticacaoEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/auth").WithTags("Autenticação");

        g.MapPost("/login", async (LoginRequest req, IValidator<LoginRequest> v,
            AutenticarUsuarioService svc, CancellationToken ct) =>
        {
            var falha = await v.ValidarOuFalharAsync(req, ct);
            if (falha is not null) return falha;
            var r = await svc.ExecutarAsync(req, ct);
            return r.ToHttpResult(val => Results.Ok(val));
        })
        .WithName("Login")
        .WithSummary("Autenticar usuário")
        .WithDescription(
            "Autentica o usuário com e-mail e senha e retorna um access token JWT e um refresh token.\n\n" +
            "**400** – campos obrigatórios ausentes ou e-mail com formato inválido.\n\n" +
            "**401** – credenciais incorretas (e-mail não cadastrado ou senha errada).")
        .Produces<AuthResponse>(200)
        .Produces<ValidacaoErroResponse>(400)
        .Produces<ErroResponse>(401);

        g.MapPost("/refresh", async (RefreshTokenRequest req,
            AutenticarUsuarioService svc, CancellationToken ct) =>
        {
            var r = await svc.RenovarAsync(req.RefreshToken, ct);
            return r.ToHttpResult(val => Results.Ok(val));
        })
        .WithName("RefreshToken")
        .WithSummary("Renovar access token")
        .WithDescription(
            "Gera um novo access token JWT a partir de um refresh token válido. " +
            "O refresh token anterior é invalidado e um novo é emitido (rotação de tokens).\n\n" +
            "**401** – refresh token inválido, expirado ou já utilizado.")
        .Produces<AuthResponse>(200)
        .Produces<ErroResponse>(401);

        g.MapPost("/logout", async (RefreshTokenRequest req,
            AutenticarUsuarioService svc, CancellationToken ct) =>
        {
            var r = await svc.LogoutAsync(req.RefreshToken, ct);
            return r.ToHttpResult();
        })
        .RequireAuthorization("UsuarioAutenticado")
        .WithName("Logout")
        .WithSummary("Encerrar sessão")
        .WithDescription(
            "Invalida o refresh token do usuário, encerrando a sessão. " +
            "O access token continuará válido até sua expiração natural.\n\n" +
            "**401** – access token ausente, inválido ou expirado.")
        .Produces(204)
        .Produces<ErroResponse>(401);
    }
}
