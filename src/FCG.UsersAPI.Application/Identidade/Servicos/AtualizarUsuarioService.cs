using FCG.UsersAPI.Application.Comum.Interfaces;
using FCG.UsersAPI.Application.Comum.Resultados;
using FCG.UsersAPI.Application.Identidade.DTOs;
using FCG.UsersAPI.Domain.Identidade.Interfaces;

namespace FCG.UsersAPI.Application.Identidade.Servicos;

public class AtualizarUsuarioService
{
    private readonly IUsuarioRepository _repo;
    private readonly IUnitOfWork _uow;

    public AtualizarUsuarioService(IUsuarioRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<Resultado<UsuarioResponse>> ExecutarAsync(Guid id, AtualizarUsuarioRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Nome))
            return Resultado<UsuarioResponse>.Falha(Erro.Validacao("Nome é obrigatório."));

        var usuario = await _repo.ObterPorIdAsync(id, ct);
        if (usuario is null)
            return Resultado<UsuarioResponse>.Falha(Erro.NaoEncontrado("Usuário não encontrado."));

        usuario.AlterarNome(req.Nome);
        _repo.Atualizar(usuario);
        await _uow.CommitAsync(ct);

        return Resultado<UsuarioResponse>.Ok(UsuarioResponse.From(usuario));
    }
}
