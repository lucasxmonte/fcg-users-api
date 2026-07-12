namespace FCG.UsersAPI.Infrastructure.Persistencia.Seed;
public class SeedAdminConfig
{
    public const string SectionName = "SeedAdmin";
    public bool Habilitado { get; set; } = true;
    public string Email { get; set; } = "admin@fcg.com";
    public string Senha { get; set; } = "TrocarEm@2026";
    public string Nome { get; set; } = "Admin FCG";
}
