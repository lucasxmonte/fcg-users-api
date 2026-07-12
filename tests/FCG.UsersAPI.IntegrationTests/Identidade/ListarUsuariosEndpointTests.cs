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

public class ListarUsuariosEndpointTests : IClassFixture<FcgUsersWebApplicationFactory>
{
    private readonly FcgUsersWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ListarUsuariosEndpointTests(FcgUsersWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ListarUsuarios_SemAutenticacao_DeveRetornar401()
    {
        var resp = await _client.GetAsync("/api/usuarios");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ListarUsuarios_UsuarioComum_DeveRetornar403()
    {
        var token = await ObterTokenAsync(admin: false);
        using var client = CriarClienteComToken(token);

        var resp = await client.GetAsync("/api/usuarios");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ListarUsuarios_Admin_DeveRetornar200ComListagem()
    {
        await CriarUsuarioNoBancoAsync("Lista Teste", $"lista-{Guid.NewGuid()}@x.com", TipoUsuario.Usuario);

        var token = await ObterTokenAsync(admin: true);
        using var client = CriarClienteComToken(token);

        var resp = await client.GetAsync("/api/usuarios");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await resp.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("total").GetInt32().Should().BeGreaterThan(0);
        body.GetProperty("pagina").GetInt32().Should().Be(1);
        body.GetProperty("tamanhoPagina").GetInt32().Should().Be(20);

        var itens = body.GetProperty("itens").EnumerateArray().ToList();
        itens.Should().NotBeEmpty();

        var primeiro = itens.First();
        primeiro.TryGetProperty("id", out _).Should().BeTrue();
        primeiro.TryGetProperty("nome", out _).Should().BeTrue();
        primeiro.TryGetProperty("email", out _).Should().BeTrue();
        primeiro.TryGetProperty("tipo", out _).Should().BeTrue();
        primeiro.TryGetProperty("dataCadastro", out _).Should().BeTrue();
        primeiro.TryGetProperty("senhaHash", out _).Should().BeFalse();
    }

    [Fact]
    public async Task ListarUsuarios_FiltrandoPorId_DeveRetornarSomenteOUsuarioCorrespondente()
    {
        var email = $"filtro-id-{Guid.NewGuid()}@x.com";
        var usuarioId = await CriarUsuarioNoBancoAsync("Filtro Id", email, TipoUsuario.Usuario);

        var token = await ObterTokenAsync(admin: true);
        using var client = CriarClienteComToken(token);

        var resp = await client.GetAsync($"/api/usuarios?id={usuarioId}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await resp.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("total").GetInt32().Should().Be(1);

        var item = body.GetProperty("itens").EnumerateArray().Single();
        item.GetProperty("id").GetGuid().Should().Be(usuarioId);
        item.GetProperty("email").GetString().Should().Be(email);
    }

    [Fact]
    public async Task ListarUsuarios_FiltrandoPorRoleUsuario_DeveRetornarSomenteUsuariosComuns()
    {
        await CriarUsuarioNoBancoAsync("Role Comum", $"role-comum-{Guid.NewGuid()}@x.com", TipoUsuario.Usuario);

        var token = await ObterTokenAsync(admin: true);
        using var client = CriarClienteComToken(token);

        var resp = await client.GetAsync("/api/usuarios?role=Usuario");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await resp.Content.ReadFromJsonAsync<JsonElement>();
        var itens = body.GetProperty("itens").EnumerateArray().ToList();

        itens.Should().NotBeEmpty();
        itens.Should().OnlyContain(u => u.GetProperty("tipo").GetString() == "Usuario");
    }

    [Fact]
    public async Task ListarUsuarios_FiltrandoPorRoleAdministrador_DeveRetornarSomenteAdmins()
    {
        var token = await ObterTokenAsync(admin: true);
        using var client = CriarClienteComToken(token);

        var resp = await client.GetAsync("/api/usuarios?role=Administrador");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await resp.Content.ReadFromJsonAsync<JsonElement>();
        var itens = body.GetProperty("itens").EnumerateArray().ToList();

        itens.Should().NotBeEmpty();
        itens.Should().OnlyContain(u => u.GetProperty("tipo").GetString() == "Administrador");
    }

    [Fact]
    public async Task ListarUsuarios_ComRoleInvalida_DeveRetornar400()
    {
        var token = await ObterTokenAsync(admin: true);
        using var client = CriarClienteComToken(token);

        var resp = await client.GetAsync("/api/usuarios?role=RoleInexistente");
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ListarUsuarios_ResponseNaoDeveExporSenhaHash()
    {
        await CriarUsuarioNoBancoAsync("Sem Senha", $"semsemha-{Guid.NewGuid()}@x.com", TipoUsuario.Usuario);

        var token = await ObterTokenAsync(admin: true);
        using var client = CriarClienteComToken(token);

        var resp = await client.GetAsync("/api/usuarios");
        var body = await resp.Content.ReadAsStringAsync();

        body.Should().NotContain("senhaHash");
        body.Should().NotContain("senhaHash");
    }

    [Fact]
    public async Task ListarUsuarios_FiltrandoPorIdInexistente_DeveRetornar404()
    {
        var token = await ObterTokenAsync(admin: true);
        using var client = CriarClienteComToken(token);

        var resp = await client.GetAsync($"/api/usuarios?id={Guid.NewGuid()}");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("Usuário não encontrado");
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
        var email = $"listar-usr-{Guid.NewGuid()}@x.com";
        const string senha = "Senha@123";

        var tipo = admin ? TipoUsuario.Administrador : TipoUsuario.Usuario;
        await CriarUsuarioNoBancoAsync(admin ? "Admin Listar" : "User Listar", email, tipo);

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
