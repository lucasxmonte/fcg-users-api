using System.Net.Http.Headers;
using FCG.UsersAPI.Application.Identidade.Interfaces;
using FCG.UsersAPI.Domain.Identidade.Entidades;
using FCG.UsersAPI.Domain.Identidade.Enums;
using FCG.UsersAPI.Domain.Identidade.Interfaces;
using FCG.UsersAPI.Domain.Identidade.ValueObjects;
using FCG.UsersAPI.Infrastructure.Persistencia;
using FCG.UsersAPI.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace FCG.UsersAPI.IntegrationTests.Identidade;

public class AuthEndpointTests : IClassFixture<FcgUsersWebApplicationFactory>
{
    private readonly FcgUsersWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthEndpointTests(FcgUsersWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // ── Login ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_SemEmail_DeveRetornar400()
    {
        var resp = await _client.PostAsJsonAsync("/api/auth/login",
            new { email = "", senha = "Senha@123" });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_SemSenha_DeveRetornar400()
    {
        var resp = await _client.PostAsJsonAsync("/api/auth/login",
            new { email = "user@x.com", senha = "" });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_EmailNaoCadastrado_DeveRetornar401()
    {
        var resp = await _client.PostAsJsonAsync("/api/auth/login",
            new { email = $"nao-existe-{Guid.NewGuid()}@x.com", senha = "Senha@123" });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_SenhaErrada_DeveRetornar401()
    {
        var email = $"auth-{Guid.NewGuid()}@x.com";
        await CriarUsuarioNoBancoAsync("Auth Teste", email, TipoUsuario.Usuario);

        var resp = await _client.PostAsJsonAsync("/api/auth/login",
            new { email, senha = "SenhaErrada@999" });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ComDadosValidos_DeveRetornar200ComTokens()
    {
        var email = $"login-{Guid.NewGuid()}@x.com";
        await CriarUsuarioNoBancoAsync("Login Teste", email, TipoUsuario.Usuario);

        var resp = await _client.PostAsJsonAsync("/api/auth/login",
            new { email, senha = "Senha@123" });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await resp.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("accessToken").GetString().Should().NotBeNullOrEmpty();
        body.GetProperty("refreshToken").GetString().Should().NotBeNullOrEmpty();
        body.TryGetProperty("senha", out _).Should().BeFalse();
        body.TryGetProperty("senhaHash", out _).Should().BeFalse();
    }

    // ── Refresh Token ──────────────────────────────────────────────────

    [Fact]
    public async Task RefreshToken_Valido_DeveRetornar200ComNovosTokens()
    {
        var email = $"refresh-{Guid.NewGuid()}@x.com";
        await CriarUsuarioNoBancoAsync("Refresh Teste", email, TipoUsuario.Usuario);

        var login = await _client.PostAsJsonAsync("/api/auth/login", new { email, senha = "Senha@123" });
        var loginBody = await login.Content.ReadFromJsonAsync<JsonElement>();
        var refreshToken = loginBody.GetProperty("refreshToken").GetString()!;

        var resp = await _client.PostAsJsonAsync("/api/auth/refresh",
            new { refreshToken });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await resp.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("accessToken").GetString().Should().NotBeNullOrEmpty();
        body.GetProperty("refreshToken").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RefreshToken_Invalido_DeveRetornar401()
    {
        var resp = await _client.PostAsJsonAsync("/api/auth/refresh",
            new { refreshToken = "token-invalido-qualquer" });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_Reutilizado_DeveRetornar401()
    {
        var email = $"reuse-{Guid.NewGuid()}@x.com";
        await CriarUsuarioNoBancoAsync("Reuse Teste", email, TipoUsuario.Usuario);

        var login = await _client.PostAsJsonAsync("/api/auth/login", new { email, senha = "Senha@123" });
        var loginBody = await login.Content.ReadFromJsonAsync<JsonElement>();
        var refreshToken = loginBody.GetProperty("refreshToken").GetString()!;

        // Primeira renovação — deve funcionar
        var primeiro = await _client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken });
        primeiro.StatusCode.Should().Be(HttpStatusCode.OK);

        // Segunda renovação com mesmo token — deve falhar
        var segundo = await _client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken });
        segundo.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Logout ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Logout_SemAutenticacao_DeveRetornar401()
    {
        var resp = await _client.PostAsJsonAsync("/api/auth/logout",
            new { refreshToken = "qualquer" });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_SemRefreshToken_DeveRetornar400()
    {
        var email = $"logout-{Guid.NewGuid()}@x.com";
        await CriarUsuarioNoBancoAsync("Logout Teste", email, TipoUsuario.Usuario);

        var login = await _client.PostAsJsonAsync("/api/auth/login", new { email, senha = "Senha@123" });
        var loginBody = await login.Content.ReadFromJsonAsync<JsonElement>();
        var accessToken = loginBody.GetProperty("accessToken").GetString()!;

        using var client = CriarClienteComToken(accessToken);
        var resp = await client.PostAsJsonAsync("/api/auth/logout",
            new { refreshToken = "" });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Logout_ComTokenInvalido_DeveRetornar400()
    {
        var email = $"logout2-{Guid.NewGuid()}@x.com";
        await CriarUsuarioNoBancoAsync("Logout Teste 2", email, TipoUsuario.Usuario);

        var login = await _client.PostAsJsonAsync("/api/auth/login", new { email, senha = "Senha@123" });
        var loginBody = await login.Content.ReadFromJsonAsync<JsonElement>();
        var accessToken = loginBody.GetProperty("accessToken").GetString()!;

        using var client = CriarClienteComToken(accessToken);
        var resp = await client.PostAsJsonAsync("/api/auth/logout",
            new { refreshToken = "token-invalido" });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── Utilitários ────────────────────────────────────────────────────

    private async Task CriarUsuarioNoBancoAsync(string nome, string email, TipoUsuario tipo)
    {
        using var scope = _factory.Services.CreateScope();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var repo   = scope.ServiceProvider.GetRequiredService<IUsuarioRepository>();
        var ctx    = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var u = new Usuario(nome, new Email(email), hasher.Gerar("Senha@123"), tipo);
        await repo.AdicionarAsync(u, default);
        await ctx.SaveChangesAsync();
    }

    private HttpClient CriarClienteComToken(string token)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
