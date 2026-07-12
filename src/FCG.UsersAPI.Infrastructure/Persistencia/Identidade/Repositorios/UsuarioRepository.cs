using Microsoft.EntityFrameworkCore;
using FCG.UsersAPI.Domain.Identidade.Entidades;
using FCG.UsersAPI.Domain.Identidade.Enums;
using FCG.UsersAPI.Domain.Identidade.Interfaces;
using FCG.UsersAPI.Infrastructure.Persistencia;
namespace FCG.UsersAPI.Infrastructure.Persistencia.Identidade.Repositorios;
public class UsuarioRepository : IUsuarioRepository
{
    private readonly ApplicationDbContext _ctx;
    public UsuarioRepository(ApplicationDbContext ctx) { _ctx = ctx; }
    public Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken ct) =>
        _ctx.Usuarios.FirstOrDefaultAsync(u => u.Id == id, ct);
    public Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken ct)
    {
        var norm = email.Trim().ToLowerInvariant();
        return _ctx.Usuarios.FirstOrDefaultAsync(u => u.Email.Valor == norm, ct);
    }
    public Task<bool> EmailExisteAsync(string email, CancellationToken ct)
    {
        var norm = email.Trim().ToLowerInvariant();
        return _ctx.Usuarios.AnyAsync(u => u.Email.Valor == norm, ct);
    }
    public async Task<(IReadOnlyList<Usuario> Itens, int Total)> ListarAsync(
        Guid? filtroId, TipoUsuario? filtroTipo, int pagina, int tamanhoPagina, CancellationToken ct)
    {
        var q = _ctx.Usuarios.AsQueryable();
        if (filtroId.HasValue) q = q.Where(u => u.Id == filtroId.Value);
        if (filtroTipo.HasValue) q = q.Where(u => u.TipoUsuario == filtroTipo.Value);
        var total = await q.CountAsync(ct);
        var itens = await q.OrderBy(u => u.Nome)
            .Skip((pagina - 1) * tamanhoPagina).Take(tamanhoPagina).ToListAsync(ct);
        return (itens, total);
    }
    public async Task AdicionarAsync(Usuario usuario, CancellationToken ct) =>
        await _ctx.Usuarios.AddAsync(usuario, ct);
    public void Atualizar(Usuario usuario) => _ctx.Usuarios.Update(usuario);
    public void Remover(Usuario usuario) => _ctx.Usuarios.Remove(usuario);
}
