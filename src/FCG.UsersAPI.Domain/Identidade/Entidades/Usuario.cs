using FCG.UsersAPI.Domain.Comum;
using FCG.UsersAPI.Domain.Identidade.Enums;
using FCG.UsersAPI.Domain.Identidade.ValueObjects;
namespace FCG.UsersAPI.Domain.Identidade.Entidades;
public class Usuario : Entidade
{
    public string Nome { get; private set; } = default!;
    public Email Email { get; private set; } = default!;
    public string SenhaHash { get; private set; } = default!;
    public TipoUsuario TipoUsuario { get; private set; }
    public DateTime DataCadastro { get; private set; }
    private Usuario() { }
    public Usuario(string nome, Email email, string senhaHash, TipoUsuario tipoUsuario)
    {
        ValidarNome(nome);
        if (email is null) throw new DomainException("E-mail é obrigatório.");
        if (string.IsNullOrWhiteSpace(senhaHash)) throw new DomainException("Hash da senha é obrigatório.");
        Nome = nome.Trim();
        Email = email;
        SenhaHash = senhaHash;
        TipoUsuario = tipoUsuario;
        DataCadastro = DateTime.UtcNow;
    }
    public void PromoverParaAdministrador()
    {
        if (TipoUsuario == TipoUsuario.Administrador) throw new DomainException("Usuário já é Administrador.");
        TipoUsuario = TipoUsuario.Administrador;
    }
    public void AlterarNome(string novoNome) { ValidarNome(novoNome); Nome = novoNome.Trim(); }
    private static void ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome)) throw new DomainException("Nome é obrigatório.");
        var n = nome.Trim();
        if (n.Length < 2 || n.Length > 100) throw new DomainException("Nome deve ter entre 2 e 100 caracteres.");
    }
}
