using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FCG.UsersAPI.Application.Identidade.Interfaces;
using FCG.UsersAPI.Infrastructure.Persistencia;
using FCG.UsersAPI.Infrastructure.Persistencia.Identidade.Entidades;
namespace FCG.UsersAPI.Infrastructure.Seguranca;
public class RefreshTokenService : IRefreshTokenService
{
    private readonly ApplicationDbContext _ctx;
    private readonly JwtConfig _cfg;
    private readonly ILogger<RefreshTokenService> _logger;
    public RefreshTokenService(ApplicationDbContext ctx, IOptions<JwtConfig> opts, ILogger<RefreshTokenService> logger)
    { _ctx = ctx; _cfg = opts.Value; _logger = logger; }

    public async Task<string> GerarAsync(Guid usuarioId, CancellationToken ct)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var entity = new RefreshTokenEntity
        {
            Id = Guid.NewGuid(), UsuarioId = usuarioId, TokenHash = ComputarHash(token),
            DataCriacao = DateTime.UtcNow, DataExpiracao = DateTime.UtcNow.AddDays(_cfg.RefreshTokenExpirationDays),
            Revogado = false
        };
        await _ctx.RefreshTokens.AddAsync(entity, ct);
        await _ctx.SaveChangesAsync(ct);
        return token;
    }

    public async Task<Guid?> ValidarERotacionarAsync(string token, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;
        var hash = ComputarHash(token);
        var atual = await _ctx.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, ct);
        if (atual is null || atual.Revogado || atual.DataExpiracao < DateTime.UtcNow) return null;
        atual.Revogado = true;
        await _ctx.SaveChangesAsync(ct);
        return atual.UsuarioId;
    }

    public async Task<bool> RevogarComValidacaoAsync(string token, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(token)) return false;
        var hash = ComputarHash(token);
        var atual = await _ctx.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, ct);
        if (atual is null || atual.Revogado) return false;
        atual.Revogado = true;
        await _ctx.SaveChangesAsync(ct);
        return true;
    }

    private static string ComputarHash(string token)
    {
        using var sha = SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(token)));
    }
}
