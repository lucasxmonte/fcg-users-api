namespace FCG.UsersAPI.Application.Comum.Interfaces;
public interface IUnitOfWork
{
    Task CommitAsync(CancellationToken ct);
}
