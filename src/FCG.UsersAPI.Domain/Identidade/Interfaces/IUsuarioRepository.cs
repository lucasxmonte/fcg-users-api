using FCG.UsersAPI.Domain.Identidade.Entidades;
using FCG.UsersAPI.Domain.Identidade.Enums;
namespace FCG.UsersAPI.Domain.Identidade.Interfaces;
public interface IUsuarioRepository
{
    Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken ct);
    Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken ct);
    Task<bool> EmailExisteAsync(string email, CancellationToken ct);
    Task<(IReadOnlyList<Usuario> Itens, int Total)> ListarAsync(Guid? filtroId, TipoUsuario? filtroTipo, int pagina, int tamanhoPagina, CancellationToken ct);
    Task AdicionarAsync(Usuario usuario, CancellationToken ct);
    void Atualizar(Usuario usuario);
    void Remover(Usuario usuario);
}
