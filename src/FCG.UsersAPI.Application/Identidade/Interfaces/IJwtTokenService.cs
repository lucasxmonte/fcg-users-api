using FCG.UsersAPI.Domain.Identidade.Entidades;
namespace FCG.UsersAPI.Application.Identidade.Interfaces;
public interface IJwtTokenService
{
    string GerarAccessToken(Usuario usuario);
    int ObterDuracaoEmSegundos();
}
