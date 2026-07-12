using FCG.UsersAPI.Application.Comum.Interfaces;
using FCG.UsersAPI.Application.Comum.Resultados;
using FCG.UsersAPI.Domain.Identidade.Interfaces;

namespace FCG.UsersAPI.Application.Identidade.Servicos;

public class RemoverUsuarioService
{
    private readonly IUsuarioRepository _repo;
    private readonly IUnitOfWork _uow;

    public RemoverUsuarioService(IUsuarioRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<Resultado> ExecutarAsync(Guid id, CancellationToken ct)
    {
        var usuario = await _repo.ObterPorIdAsync(id, ct);
        if (usuario is null)
            return Resultado.Falha(Erro.NaoEncontrado("Usuário não encontrado."));

        _repo.Remover(usuario);
        await _uow.CommitAsync(ct);

        return Resultado.Ok();
    }
}
