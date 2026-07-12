using System.Text.RegularExpressions;
using FCG.UsersAPI.Domain.Comum;
namespace FCG.UsersAPI.Domain.Identidade.ValueObjects;
public sealed class Senha : ValueObject
{
    public const int TamanhoMinimo = 8;
    public string Valor { get; }
    public Senha(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor)) throw new DomainException("Senha é obrigatória.");
        if (valor.Length < TamanhoMinimo) throw new DomainException($"Senha deve ter no mínimo {TamanhoMinimo} caracteres.");
        if (!Regex.IsMatch(valor, @"[A-Za-z]")) throw new DomainException("Senha deve conter ao menos uma letra.");
        if (!Regex.IsMatch(valor, @"[0-9]")) throw new DomainException("Senha deve conter ao menos um número.");
        if (!Regex.IsMatch(valor, @"[^A-Za-z0-9]")) throw new DomainException("Senha deve conter ao menos um caractere especial.");
        Valor = valor;
    }
    protected override IEnumerable<object?> ObterAtributosDeIgualdade() { yield return Valor; }
}
