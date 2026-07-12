using FCG.UsersAPI.Domain.Identidade.Entidades;
namespace FCG.UsersAPI.Application.Identidade.DTOs;
public record UsuarioResponse(Guid Id, string Nome, string Email, string Tipo, DateTime DataCadastro)
{
    public static UsuarioResponse From(Usuario u) =>
        new(u.Id, u.Nome, u.Email.Valor, u.TipoUsuario.ToString(), u.DataCadastro);
}
