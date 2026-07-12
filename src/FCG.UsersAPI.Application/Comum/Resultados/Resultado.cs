namespace FCG.UsersAPI.Application.Comum.Resultados;
public sealed class Resultado<T>
{
    public bool Sucesso { get; }
    public T? Valor { get; }
    public Erro? Erro { get; }
    private Resultado(bool sucesso, T? valor, Erro? erro) { Sucesso = sucesso; Valor = valor; Erro = erro; }
    public static Resultado<T> Ok(T valor) => new(true, valor, null);
    public static Resultado<T> Falha(Erro erro) => new(false, default, erro);
}
public sealed class Resultado
{
    public bool Sucesso { get; }
    public Erro? Erro { get; }
    private Resultado(bool sucesso, Erro? erro) { Sucesso = sucesso; Erro = erro; }
    public static Resultado Ok() => new(true, null);
    public static Resultado Falha(Erro erro) => new(false, erro);
}
