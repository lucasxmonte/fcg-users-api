using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FCG.UsersAPI.Infrastructure.Persistencia.Identidade.Entidades;
namespace FCG.UsersAPI.Infrastructure.Persistencia.Identidade.Configuracoes;
internal class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshTokenEntity>
{
    public void Configure(EntityTypeBuilder<RefreshTokenEntity> b)
    {
        b.ToTable("refresh_tokens", "identidade");
        b.HasKey(r => r.Id);
        b.Property(r => r.Id).HasColumnName("id");
        b.Property(r => r.UsuarioId).HasColumnName("usuario_id").IsRequired();
        b.Property(r => r.TokenHash).HasColumnName("token_hash").IsRequired().HasMaxLength(200);
        b.Property(r => r.DataExpiracao).HasColumnName("data_expiracao").IsRequired();
        b.Property(r => r.DataCriacao).HasColumnName("data_criacao").IsRequired();
        b.Property(r => r.Revogado).HasColumnName("revogado").HasDefaultValue(false);
        b.HasIndex(r => r.TokenHash).IsUnique().HasDatabaseName("ix_refresh_tokens_token_hash");
        b.HasIndex(r => new { r.UsuarioId, r.Revogado }).HasDatabaseName("ix_refresh_tokens_usuario_revogado");
    }
}
