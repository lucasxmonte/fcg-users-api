namespace FCG.UsersAPI.Application.Identidade.Interfaces;
public interface IRefreshTokenService
{
    Task<string> GerarAsync(Guid usuarioId, CancellationToken ct);
    Task<Guid?> ValidarERotacionarAsync(string token, CancellationToken ct);
    Task<bool> RevogarComValidacaoAsync(string token, CancellationToken ct);
}
