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

public class RegistrarUsuarioEndpointTests : IClassFixture<FcgUsersWebApplicationFactory>
{
    private readonly FcgUsersWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public RegistrarUsuarioEndpointTests(FcgUsersWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // ── REGISTRAR USUÁRIO COMUM (público) ──────────────────────────────

    [Fact]
    public async Task RegistrarUsuario_ComDadosValidos_DeveRetornar201()
    {
        var req = new
        {
            nome = "Maria Teste",
            email = $"maria-{Guid.NewGuid()}@x.com",
            senha = "Senha@123"
        };

        var resp = await _client.PostAsJsonAsync("/api/usuarios/registrar", req);

        resp.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await resp.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("nome").GetString().Should().Be("Maria Teste");
        body.GetProperty("tipo").GetString().Should().Be("Usuario");
        body.TryGetProperty("senha", out _).Should().BeFalse();
        body.TryGetProperty("senhaHash", out _).Should().BeFalse();
    }

    [Fact]
    public async Task RegistrarUsuario_ComEmailDuplicado_DeveRetornar409()
    {
        var email = $"dup-{Guid.NewGuid()}@x.com";
        var req = new { nome = "Dup Teste", email, senha = "Senha@123" };

        var primeiro = await _client.PostAsJsonAsync("/api/usuarios/registrar", req);
        primeiro.StatusCode.Should().Be(HttpStatusCode.Created);

        var segundo = await _client.PostAsJsonAsync("/api/usuarios/registrar", req);
        segundo.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var body = await segundo.Content.ReadAsStringAsync();
        body.Should().Contain("já");
    }

    [Fact]
    public async Task RegistrarUsuario_SemEmail_DeveRetornar400ComMensagem()
    {
        var req = new { nome = "Maria Teste", email = "", senha = "Senha@123" };
        var resp = await _client.PostAsJsonAsync("/api/usuarios/registrar", req);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("Campo e-mail obrigat");
    }

    [Fact]
    public async Task RegistrarUsuario_SemSenha_DeveRetornar400ComMensagem()
    {
        var req = new { nome = "Maria Teste", email = $"test-{Guid.NewGuid()}@x.com", senha = "" };
        var resp = await _client.PostAsJsonAsync("/api/usuarios/registrar", req);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("Campo senha obrigat");
    }

    [Fact]
    public async Task RegistrarUsuario_SemNome_DeveRetornar400ComMensagem()
    {
        var req = new { nome = "", email = $"test-{Guid.NewGuid()}@x.com", senha = "Senha@123" };
        var resp = await _client.PostAsJsonAsync("/api/usuarios/registrar", req);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("Campo nome obrigat");
    }

    [Fact]
    public async Task RegistrarUsuario_ComEmailInvalido_DeveRetornar400ComMensagem()
    {
        var req = new { nome = "Maria Teste", email = "naoEhEmail", senha = "Senha@123" };
        var resp = await _client.PostAsJsonAsync("/api/usuarios/registrar", req);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().ContainAny("invalido", "inválido", "E-mail");
    }

    [Fact]
    public async Task RegistrarUsuario_ComEmailComCaracteresInvalidos_DeveRetornar400ComMensagem()
    {
        var req = new { nome = "Maria Teste", email = "email invalido@x.com", senha = "Senha@123" };
        var resp = await _client.PostAsJsonAsync("/api/usuarios/registrar", req);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().ContainAny("e-mail", "invalido", "inválido");
    }

    [Fact]
    public async Task RegistrarUsuario_SomenteUmNome_DeveRetornar400ComMensagem()
    {
        var req = new { nome = "Maria", email = $"test-{Guid.NewGuid()}@x.com", senha = "Senha@123" };
        var resp = await _client.PostAsJsonAsync("/api/usuarios/registrar", req);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("sobrenome");
    }

    [Fact]
    public async Task RegistrarUsuario_NomeComCaracteresEspeciais_DeveRetornar400ComMensagem()
    {
        var req = new { nome = "Maria 123", email = $"test-{Guid.NewGuid()}@x.com", senha = "Senha@123" };
        var resp = await _client.PostAsJsonAsync("/api/usuarios/registrar", req);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("somente letras");
    }

    [Fact]
    public async Task RegistrarUsuario_SenhaSemMaiuscula_DeveRetornar400ComMensagem()
    {
        var req = new { nome = "Maria Teste", email = $"test-{Guid.NewGuid()}@x.com", senha = "senha@123" };
        var resp = await _client.PostAsJsonAsync("/api/usuarios/registrar", req);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().ContainAny("maiúscula", "maiuscula");
    }

    [Fact]
    public async Task RegistrarUsuario_SenhaSemMinuscula_DeveRetornar400ComMensagem()
    {
        var req = new { nome = "Maria Teste", email = $"test-{Guid.NewGuid()}@x.com", senha = "SENHA@123" };
        var resp = await _client.PostAsJsonAsync("/api/usuarios/registrar", req);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().ContainAny("minúscula", "minuscula");
    }

    [Fact]
    public async Task RegistrarUsuario_SenhaSemCaractereEspecial_DeveRetornar400ComMensagem()
    {
        var req = new { nome = "Maria Teste", email = $"test-{Guid.NewGuid()}@x.com", senha = "Senha1234" };
        var resp = await _client.PostAsJsonAsync("/api/usuarios/registrar", req);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("especial");
    }

    [Fact]
    public async Task RegistrarUsuario_SenhaSemNumero_DeveRetornar400ComMensagem()
    {
        var req = new { nome = "Maria Teste", email = $"test-{Guid.NewGuid()}@x.com", senha = "Senha@abc" };
        var resp = await _client.PostAsJsonAsync("/api/usuarios/registrar", req);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().ContainAny("número", "numero");
    }

    // ── REGISTRAR ADMINISTRADOR (requer AdminOnly) ─────────────────────

    [Fact]
    public async Task RegistrarAdmin_SemAutenticacao_DeveRetornar401()
    {
        var req = new { nome = "Admin Teste", email = $"admin-{Guid.NewGuid()}@x.com", senha = "Senha@123" };
        var resp = await _client.PostAsJsonAsync("/api/usuarios/administradores", req);
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RegistrarAdmin_UsuarioComum_DeveRetornar403()
    {
        var token = await ObterTokenAsync(admin: false);
        using var client = CriarClienteComToken(token);

        var req = new { nome = "Admin Teste", email = $"admin-{Guid.NewGuid()}@x.com", senha = "Senha@123" };
        var resp = await client.PostAsJsonAsync("/api/usuarios/administradores", req);
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RegistrarAdmin_Admin_DeveRetornar201()
    {
        var token = await ObterTokenAsync(admin: true);
        using var client = CriarClienteComToken(token);

        var req = new { nome = "Admin Novo", email = $"admin-novo-{Guid.NewGuid()}@x.com", senha = "Senha@123" };
        var resp = await client.PostAsJsonAsync("/api/usuarios/administradores", req);
        resp.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await resp.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("tipo").GetString().Should().Be("Administrador");
    }

    [Fact]
    public async Task RegistrarAdmin_ComEmailDuplicado_DeveRetornar409()
    {
        var token = await ObterTokenAsync(admin: true);
        using var client = CriarClienteComToken(token);

        var email = $"admin-dup-{Guid.NewGuid()}@x.com";
        var req = new { nome = "Admin Dup", email, senha = "Senha@123" };

        var primeiro = await client.PostAsJsonAsync("/api/usuarios/administradores", req);
        primeiro.StatusCode.Should().Be(HttpStatusCode.Created);

        var segundo = await client.PostAsJsonAsync("/api/usuarios/administradores", req);
        segundo.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var body = await segundo.Content.ReadAsStringAsync();
        body.Should().Contain("já");
    }

    [Fact]
    public async Task RegistrarAdmin_ComEmailInvalido_DeveRetornar400()
    {
        var token = await ObterTokenAsync(admin: true);
        using var client = CriarClienteComToken(token);

        var req = new { nome = "Admin Teste", email = "naoEhEmail", senha = "Senha@123" };
        var resp = await client.PostAsJsonAsync("/api/usuarios/administradores", req);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await resp.Content.ReadAsStringAsync();
        body.Should().ContainAny("invalido", "inválido", "E-mail");
    }

    [Fact]
    public async Task RegistrarAdmin_SemEmail_DeveRetornar400()
    {
        var token = await ObterTokenAsync(admin: true);
        using var client = CriarClienteComToken(token);

        var req = new { nome = "Admin Teste", email = "", senha = "Senha@123" };
        var resp = await client.PostAsJsonAsync("/api/usuarios/administradores", req);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("Campo e-mail obrigat");
    }

    [Fact]
    public async Task RegistrarAdmin_SemNome_DeveRetornar400()
    {
        var token = await ObterTokenAsync(admin: true);
        using var client = CriarClienteComToken(token);

        var req = new { nome = "", email = $"test-{Guid.NewGuid()}@x.com", senha = "Senha@123" };
        var resp = await client.PostAsJsonAsync("/api/usuarios/administradores", req);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("Campo nome obrigat");
    }

    [Fact]
    public async Task RegistrarAdmin_SemSenha_DeveRetornar400()
    {
        var token = await ObterTokenAsync(admin: true);
        using var client = CriarClienteComToken(token);

        var req = new { nome = "Admin Teste", email = $"test-{Guid.NewGuid()}@x.com", senha = "" };
        var resp = await client.PostAsJsonAsync("/api/usuarios/administradores", req);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("Campo senha obrigat");
    }

    [Fact]
    public async Task RegistrarAdmin_SomenteUmNome_DeveRetornar400()
    {
        var token = await ObterTokenAsync(admin: true);
        using var client = CriarClienteComToken(token);

        var req = new { nome = "Admin", email = $"test-{Guid.NewGuid()}@x.com", senha = "Senha@123" };
        var resp = await client.PostAsJsonAsync("/api/usuarios/administradores", req);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("sobrenome");
    }

    [Fact]
    public async Task RegistrarAdmin_NomeComNumerais_DeveRetornar400()
    {
        var token = await ObterTokenAsync(admin: true);
        using var client = CriarClienteComToken(token);

        var req = new { nome = "Admin 123", email = $"test-{Guid.NewGuid()}@x.com", senha = "Senha@123" };
        var resp = await client.PostAsJsonAsync("/api/usuarios/administradores", req);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("somente letras");
    }

    [Fact]
    public async Task RegistrarAdmin_SenhaSemMaiuscula_DeveRetornar400()
    {
        var token = await ObterTokenAsync(admin: true);
        using var client = CriarClienteComToken(token);

        var req = new { nome = "Admin Teste", email = $"test-{Guid.NewGuid()}@x.com", senha = "senha@123" };
        var resp = await client.PostAsJsonAsync("/api/usuarios/administradores", req);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await resp.Content.ReadAsStringAsync();
        body.Should().ContainAny("maiúscula", "maiuscula");
    }

    [Fact]
    public async Task RegistrarAdmin_SenhaSemMinuscula_DeveRetornar400()
    {
        var token = await ObterTokenAsync(admin: true);
        using var client = CriarClienteComToken(token);

        var req = new { nome = "Admin Teste", email = $"test-{Guid.NewGuid()}@x.com", senha = "SENHA@123" };
        var resp = await client.PostAsJsonAsync("/api/usuarios/administradores", req);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await resp.Content.ReadAsStringAsync();
        body.Should().ContainAny("minúscula", "minuscula");
    }

    [Fact]
    public async Task RegistrarAdmin_SenhaSemCaractereEspecial_DeveRetornar400()
    {
        var token = await ObterTokenAsync(admin: true);
        using var client = CriarClienteComToken(token);

        var req = new { nome = "Admin Teste", email = $"test-{Guid.NewGuid()}@x.com", senha = "Senha1234" };
        var resp = await client.PostAsJsonAsync("/api/usuarios/administradores", req);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("especial");
    }

    [Fact]
    public async Task RegistrarAdmin_SenhaSemNumero_DeveRetornar400()
    {
        var token = await ObterTokenAsync(admin: true);
        using var client = CriarClienteComToken(token);

        var req = new { nome = "Admin Teste", email = $"test-{Guid.NewGuid()}@x.com", senha = "Senha@abc" };
        var resp = await client.PostAsJsonAsync("/api/usuarios/administradores", req);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await resp.Content.ReadAsStringAsync();
        body.Should().ContainAny("número", "numero");
    }

    // ── PROMOVER USUÁRIO ───────────────────────────────────────────────

    [Fact]
    public async Task PromoverUsuario_SemAutenticacao_DeveRetornar401()
    {
        var resp = await _client.PatchAsync($"/api/usuarios/{Guid.NewGuid()}/promover", null);
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PromoverUsuario_UsuarioComum_DeveRetornar403()
    {
        var token = await ObterTokenAsync(admin: false);
        using var client = CriarClienteComToken(token);

        var resp = await client.PatchAsync($"/api/usuarios/{Guid.NewGuid()}/promover", null);
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PromoverUsuario_IdInexistente_DeveRetornar404ComMensagem()
    {
        var token = await ObterTokenAsync(admin: true);
        using var client = CriarClienteComToken(token);

        var resp = await client.PatchAsync($"/api/usuarios/{Guid.NewGuid()}/promover", null);
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var body = await resp.Content.ReadAsStringAsync();
        body.Should().ContainAny("não encontrado", "nao encontrado");
    }

    [Fact]
    public async Task PromoverUsuario_Admin_DeveRetornar200EPromoverUsuario()
    {
        var token = await ObterTokenAsync(admin: true);
        using var client = CriarClienteComToken(token);

        var usuarioId = await CriarUsuarioNoBancoAsync("Usuario Promover", $"promover-{Guid.NewGuid()}@x.com", TipoUsuario.Usuario);

        var resp = await client.PatchAsync($"/api/usuarios/{usuarioId}/promover", null);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await resp.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("tipo").GetString().Should().Be("Administrador");
    }

    // ── Utilitários ────────────────────────────────────────────────────

    private async Task<Guid> CriarUsuarioNoBancoAsync(string nome, string email, TipoUsuario tipo)
    {
        using var scope = _factory.Services.CreateScope();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var repo   = scope.ServiceProvider.GetRequiredService<IUsuarioRepository>();
        var ctx    = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var u = new Usuario(nome, new Email(email), hasher.Gerar("Senha@123"), tipo);
        await repo.AdicionarAsync(u, default);
        await ctx.SaveChangesAsync();
        return u.Id;
    }

    private async Task<string> ObterTokenAsync(bool admin)
    {
        var email = $"reg-usr-{Guid.NewGuid()}@x.com";
        const string senha = "Senha@123";

        var tipo = admin ? TipoUsuario.Administrador : TipoUsuario.Usuario;
        await CriarUsuarioNoBancoAsync(admin ? "Admin Registrar" : "User Registrar", email, tipo);

        var login = await _client.PostAsJsonAsync("/api/auth/login", new { email, senha });
        login.EnsureSuccessStatusCode();
        var body = await login.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("accessToken").GetString()!;
    }

    private HttpClient CriarClienteComToken(string token)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
