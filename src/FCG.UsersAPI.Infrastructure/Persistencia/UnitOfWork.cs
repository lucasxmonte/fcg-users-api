using FCG.UsersAPI.Application.Comum.Interfaces;
using FCG.UsersAPI.Infrastructure.Persistencia;
namespace FCG.UsersAPI.Infrastructure.Persistencia;
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _ctx;
    public UnitOfWork(ApplicationDbContext ctx) { _ctx = ctx; }
    public Task CommitAsync(CancellationToken ct) => _ctx.SaveChangesAsync(ct);
}
