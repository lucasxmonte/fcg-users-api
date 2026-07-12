using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using FCG.UsersAPI.Application.Identidade.Interfaces;
using FCG.UsersAPI.Domain.Identidade.Entidades;
namespace FCG.UsersAPI.Infrastructure.Seguranca;
public class JwtTokenService : IJwtTokenService
{
    private readonly JwtConfig _cfg;
    public JwtTokenService(IOptions<JwtConfig> opts) { _cfg = opts.Value; }
    public string GerarAccessToken(Usuario usuario)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg.Secret)) { KeyId = "fcg-signing-key" };
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Email.Valor),
            new("nome", usuario.Nome),
            new(ClaimTypes.Role, usuario.TipoUsuario.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var token = new JwtSecurityToken(issuer: _cfg.Issuer, audience: _cfg.Audience,
            claims: claims, notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(_cfg.AccessTokenExpirationMinutes),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    public int ObterDuracaoEmSegundos() => _cfg.AccessTokenExpirationMinutes * 60;
}
