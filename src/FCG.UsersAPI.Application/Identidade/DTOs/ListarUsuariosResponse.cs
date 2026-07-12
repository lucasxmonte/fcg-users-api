namespace FCG.UsersAPI.Application.Identidade.DTOs;
public record ListarUsuariosResponse(IReadOnlyList<UsuarioResponse> Itens, int Total, int Pagina, int TamanhoPagina);
