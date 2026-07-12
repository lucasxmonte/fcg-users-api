namespace FCG.UsersAPI.Application.Identidade.DTOs;
public record AuthResponse(string AccessToken, string RefreshToken, int ExpiresIn, string TokenType, UsuarioResponse Usuario);
