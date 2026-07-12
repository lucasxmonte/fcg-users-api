using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FCG.UsersAPI.Application.Identidade.Interfaces;
using FCG.UsersAPI.Domain.Identidade.Entidades;
using FCG.UsersAPI.Domain.Identidade.Enums;
using FCG.UsersAPI.Domain.Identidade.Interfaces;
using FCG.UsersAPI.Domain.Identidade.ValueObjects;
using FCG.UsersAPI.Infrastructure.Persistencia;
namespace FCG.UsersAPI.Infrastructure.Persistencia.Seed;
public class SeedData
{
    private readonly IUsuarioRepository _repo;
    private readonly IPasswordHasher _hasher;
    private readonly ApplicationDbContext _ctx;
    private readonly SeedAdminConfig _cfg;
    private readonly ILogger<SeedData> _logger;
    public SeedData(IUsuarioRepository repo, IPasswordHasher hasher, ApplicationDbContext ctx,
        IOptions<SeedAdminConfig> cfg, ILogger<SeedData> logger)
    { _repo = repo; _hasher = hasher; _ctx = ctx; _cfg = cfg.Value; _logger = logger; }

    public async Task ExecutarAsync(CancellationToken ct = default)
    {
        if (!_cfg.Habilitado) { _logger.LogInformation("Seed do Admin desabilitado."); return; }
        if (await _repo.EmailExisteAsync(_cfg.Email, ct))
        { _logger.LogInformation("Seed Admin: usuário já existe ({Email}).", _cfg.Email); return; }
        var email = new Email(_cfg.Email);
        var hash = _hasher.Gerar(_cfg.Senha);
        var admin = new Usuario(_cfg.Nome, email, hash, TipoUsuario.Administrador);
        await _repo.AdicionarAsync(admin, ct);
        await _ctx.SaveChangesAsync(ct);
        _logger.LogWarning("Seed Admin criado. Email={Email}. ALTERE A SENHA APÓS O PRIMEIRO LOGIN!", _cfg.Email);
    }
}
