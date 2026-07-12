using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FCG.UsersAPI.Domain.Identidade.Entidades;
namespace FCG.UsersAPI.Infrastructure.Persistencia.Identidade.Configuracoes;
public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> b)
    {
        b.ToTable("usuarios", "identidade");
        b.HasKey(u => u.Id);
        b.Property(u => u.Id).HasColumnName("id");
        b.Property(u => u.Nome).HasColumnName("nome").IsRequired().HasMaxLength(100);
        b.OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Valor).HasColumnName("email").IsRequired().HasMaxLength(200);
            email.HasIndex(e => e.Valor).IsUnique().HasDatabaseName("ix_usuarios_email");
        });
        b.Property(u => u.SenhaHash).HasColumnName("senha_hash").IsRequired().HasMaxLength(200);
        b.Property(u => u.TipoUsuario).HasColumnName("tipo_usuario").HasConversion<string>().HasMaxLength(20).IsRequired();
        b.Property(u => u.DataCadastro).HasColumnName("data_cadastro").IsRequired();
    }
}
