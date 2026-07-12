using System.Text.RegularExpressions;
using FCG.UsersAPI.Domain.Comum;
namespace FCG.UsersAPI.Domain.Identidade.ValueObjects;
public sealed class Email : ValueObject
{
    private static readonly Regex Padrao = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    public string Valor { get; }
    public Email(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor)) throw new DomainException("E-mail é obrigatório.");
        var normalizado = valor.Trim().ToLowerInvariant();
        if (!Padrao.IsMatch(normalizado)) throw new DomainException("E-mail inválido.");
        Valor = normalizado;
    }
    private Email() { Valor = string.Empty; }
    protected override IEnumerable<object?> ObterAtributosDeIgualdade() { yield return Valor; }
    public override string ToString() => Valor;
}
