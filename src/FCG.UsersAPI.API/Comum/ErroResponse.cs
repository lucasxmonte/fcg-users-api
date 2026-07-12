namespace FCG.UsersAPI.API.Comum;

/// <summary>Resposta padrão de erro com mensagem única.</summary>
/// <param name="Erro">Descrição do erro ocorrido.</param>
public record ErroResponse(string Erro);

/// <summary>Resposta de erro de validação com lista de mensagens.</summary>
/// <param name="Erros">Lista de mensagens de validação.</param>
public record ValidacaoErroResponse(List<string> Erros);
