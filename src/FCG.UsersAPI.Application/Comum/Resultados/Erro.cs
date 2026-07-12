namespace FCG.UsersAPI.Application.Comum.Resultados;
public sealed record Erro(string Codigo, string Mensagem)
{
    public static Erro NaoEncontrado(string m)   => new("NAO_ENCONTRADO", m);
    public static Erro Conflito(string m)         => new("CONFLITO", m);
    public static Erro Validacao(string m)        => new("VALIDACAO", m);
    public static Erro NaoAutenticado(string m)   => new("NAO_AUTENTICADO", m);
}
