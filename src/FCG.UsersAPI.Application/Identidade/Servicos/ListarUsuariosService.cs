using FCG.UsersAPI.Application.Comum.Resultados;
using FCG.UsersAPI.Application.Identidade.DTOs;
using FCG.UsersAPI.Domain.Identidade.Enums;
using FCG.UsersAPI.Domain.Identidade.Interfaces;
namespace FCG.UsersAPI.Application.Identidade.Servicos;
public class ListarUsuariosService
{
    private readonly IUsuarioRepository _repo;
    public ListarUsuariosService(IUsuarioRepository repo) { _repo = repo; }
    public async Task<Resultado<ListarUsuariosResponse>> ExecutarAsync(
        Guid? filtroId, TipoUsuario? filtroTipo, int pagina, int tamanhoPagina, CancellationToken ct)
    {
        if (pagina < 1) pagina = 1;
        if (tamanhoPagina < 1 || tamanhoPagina > 100) tamanhoPagina = 20;
        var (itens, total) = await _repo.ListarAsync(filtroId, filtroTipo, pagina, tamanhoPagina, ct);
        if (filtroId.HasValue && total == 0)
            return Resultado<ListarUsuariosResponse>.Falha(Erro.NaoEncontrado("Usuário não encontrado"));
        return Resultado<ListarUsuariosResponse>.Ok(new ListarUsuariosResponse(
            itens.Select(UsuarioResponse.From).ToList(), total, pagina, tamanhoPagina));
    }
}
