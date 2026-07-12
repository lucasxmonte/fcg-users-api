namespace FCG.UsersAPI.Infrastructure.Persistencia.Identidade.Entidades;
internal class RefreshTokenEntity
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public string TokenHash { get; set; } = default!;
    public DateTime DataExpiracao { get; set; }
    public DateTime DataCriacao { get; set; }
    public bool Revogado { get; set; }
}
