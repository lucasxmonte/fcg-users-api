using Microsoft.EntityFrameworkCore;
using FCG.UsersAPI.Domain.Identidade.Entidades;
using FCG.UsersAPI.Infrastructure.Persistencia.Identidade.Entidades;
namespace FCG.UsersAPI.Infrastructure.Persistencia;
public class ApplicationDbContext : DbContext
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    internal DbSet<RefreshTokenEntity> RefreshTokens => Set<RefreshTokenEntity>();
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("identidade");
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(builder);
    }
}
