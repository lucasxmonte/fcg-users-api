using FCG.UsersAPI.Application.Identidade.Interfaces;
namespace FCG.UsersAPI.Infrastructure.Seguranca;
public class BCryptPasswordHasher : IPasswordHasher
{
    public string Gerar(string senha) => BCrypt.Net.BCrypt.HashPassword(senha, workFactor: 12);
    public bool Verificar(string senha, string hash) => BCrypt.Net.BCrypt.Verify(senha, hash);
}
