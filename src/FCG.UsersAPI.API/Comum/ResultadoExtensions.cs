using FCG.UsersAPI.Application.Comum.Resultados;
namespace FCG.UsersAPI.API.Comum;
public static class ResultadoExtensions
{
    public static IResult ToHttpResult<T>(this Resultado<T> r, Func<T, IResult> onSuccess)
    {
        if (r.Sucesso) return onSuccess(r.Valor!);
        return ConverterErro(r.Erro!);
    }
    public static IResult ToHttpResult(this Resultado r)
    {
        if (r.Sucesso) return Results.NoContent();
        return ConverterErro(r.Erro!);
    }
    private static IResult ConverterErro(Erro erro) => erro.Codigo switch
    {
        "NAO_ENCONTRADO"  => Results.NotFound(new ErroResponse(erro.Mensagem)),
        "CONFLITO"        => Results.Conflict(new ErroResponse(erro.Mensagem)),
        "VALIDACAO"       => Results.BadRequest(new ErroResponse(erro.Mensagem)),
        "NAO_AUTENTICADO" => Results.Json(new ErroResponse(erro.Mensagem), statusCode: 401),
        _                 => Results.Problem(erro.Mensagem)
    };
}
