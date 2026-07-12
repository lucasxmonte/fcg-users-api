using Microsoft.Extensions.Logging;
using FCG.UsersAPI.Application.Comum.Interfaces;
using FCG.UsersAPI.Application.Comum.Resultados;
using FCG.UsersAPI.Application.Identidade.DTOs;
using FCG.UsersAPI.Domain.Comum;
using FCG.UsersAPI.Domain.Identidade.Interfaces;
namespace FCG.UsersAPI.Application.Identidade.Servicos;
public class PromoverUsuarioService
{
    private readonly IUsuarioRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<PromoverUsuarioService> _logger;
    public PromoverUsuarioService(IUsuarioRepository repo, IUnitOfWork uow, ILogger<PromoverUsuarioService> logger)
    { _repo = repo; _uow = uow; _logger = logger; }
    public async Task<Resultado<UsuarioResponse>> ExecutarAsync(Guid usuarioId, CancellationToken ct)
    {
        var usuario = await _repo.ObterPorIdAsync(usuarioId, ct);
        if (usuario is null)
            return Resultado<UsuarioResponse>.Falha(Erro.NaoEncontrado("Usuário não encontrado."));
        try { usuario.PromoverParaAdministrador(); }
        catch (DomainException ex) { return Resultado<UsuarioResponse>.Falha(Erro.Validacao(ex.Message)); }
        _repo.Atualizar(usuario);
        await _uow.CommitAsync(ct);
        _logger.LogInformation("Usuario promovido a admin. Id={UsuarioId}", usuario.Id);
        return Resultado<UsuarioResponse>.Ok(UsuarioResponse.From(usuario));
    }
}
