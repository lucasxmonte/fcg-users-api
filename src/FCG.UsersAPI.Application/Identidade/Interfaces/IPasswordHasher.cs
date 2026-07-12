namespace FCG.UsersAPI.Application.Identidade.Interfaces;
public interface IPasswordHasher
{
    string Gerar(string senha);
    bool Verificar(string senha, string hash);
}
