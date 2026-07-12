using Microsoft.Extensions.Logging;
using FCG.UsersAPI.Application.Comum.Resultados;
using FCG.UsersAPI.Application.Identidade.DTOs;
using FCG.UsersAPI.Application.Identidade.Interfaces;
using FCG.UsersAPI.Domain.Identidade.Interfaces;
namespace FCG.UsersAPI.Application.Identidade.Servicos;
public class AutenticarUsuarioService
{
    private readonly IUsuarioRepository _repo;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenService _jwt;
    private readonly IRefreshTokenService _refresh;
    private readonly ILogger<AutenticarUsuarioService> _logger;
    public AutenticarUsuarioService(IUsuarioRepository repo, IPasswordHasher hasher,
        IJwtTokenService jwt, IRefreshTokenService refresh, ILogger<AutenticarUsuarioService> logger)
    { _repo = repo; _hasher = hasher; _jwt = jwt; _refresh = refresh; _logger = logger; }

    public async Task<Resultado<AuthResponse>> ExecutarAsync(LoginRequest req, CancellationToken ct)
    {
        var emailNorm = req.Email.Trim().ToLowerInvariant();
        var usuario = await _repo.ObterPorEmailAsync(emailNorm, ct);
        if (usuario is null || !_hasher.Verificar(req.Senha, usuario.SenhaHash))
        {
            _logger.LogWarning("Login falhou. Email={Email}", emailNorm);
            return Resultado<AuthResponse>.Falha(Erro.NaoAutenticado("Credenciais inválidas."));
        }
        var accessToken = _jwt.GerarAccessToken(usuario);
        var refreshToken = await _refresh.GerarAsync(usuario.Id, ct);
        _logger.LogInformation("Usuario autenticado. Id={UsuarioId}", usuario.Id);
        return Resultado<AuthResponse>.Ok(new AuthResponse(accessToken, refreshToken,
            _jwt.ObterDuracaoEmSegundos(), "Bearer", UsuarioResponse.From(usuario)));
    }

    public async Task<Resultado<AuthResponse>> RenovarAsync(string refreshToken, CancellationToken ct)
    {
        var usuarioId = await _refresh.ValidarERotacionarAsync(refreshToken, ct);
        if (usuarioId is null)
            return Resultado<AuthResponse>.Falha(Erro.NaoAutenticado("Refresh token incorreto ou invalido"));
        var usuario = await _repo.ObterPorIdAsync(usuarioId.Value, ct);
        if (usuario is null)
            return Resultado<AuthResponse>.Falha(Erro.NaoAutenticado("Refresh token incorreto ou invalido"));
        var newAccess = _jwt.GerarAccessToken(usuario);
        var newRefresh = await _refresh.GerarAsync(usuario.Id, ct);
        return Resultado<AuthResponse>.Ok(new AuthResponse(newAccess, newRefresh,
            _jwt.ObterDuracaoEmSegundos(), "Bearer", UsuarioResponse.From(usuario)));
    }

    public async Task<Resultado> LogoutAsync(string refreshToken, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return Resultado.Falha(Erro.Validacao("Não foi possível realizar o logout devido a ausência do refresh token"));
        var revogado = await _refresh.RevogarComValidacaoAsync(refreshToken, ct);
        if (!revogado)
            return Resultado.Falha(Erro.Validacao("Não foi possível realizar o logout, refresh token incorreto/invalido"));
        return Resultado.Ok();
    }
}
