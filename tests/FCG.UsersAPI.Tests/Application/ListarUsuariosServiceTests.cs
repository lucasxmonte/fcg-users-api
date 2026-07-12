using FluentAssertions;
using Moq;
using FCG.UsersAPI.Application.Identidade.Servicos;
using FCG.UsersAPI.Domain.Identidade.Entidades;
using FCG.UsersAPI.Domain.Identidade.Enums;
using FCG.UsersAPI.Domain.Identidade.Interfaces;
using FCG.UsersAPI.Domain.Identidade.ValueObjects;

namespace FCG.UsersAPI.Tests.Application;

public class ListarUsuariosServiceTests
{
    private readonly Mock<IUsuarioRepository> _repo = new();

    private ListarUsuariosService Servico() => new(_repo.Object);

    private static Usuario UsuarioMock(string nome, string email, TipoUsuario tipo) =>
        new(nome, new Email(email), "hash", tipo);

    [Fact]
    public async Task Listar_SemFiltros_DeveRetornarTodosOsUsuarios()
    {
        var usuarios = new List<Usuario>
        {
            UsuarioMock("Alice", "alice@x.com", TipoUsuario.Usuario),
            UsuarioMock("Bob",   "bob@x.com",   TipoUsuario.Administrador)
        };

        _repo.Setup(r => r.ListarAsync(null, null, 1, 20, It.IsAny<CancellationToken>()))
             .ReturnsAsync((usuarios, 2));

        var resultado = await Servico().ExecutarAsync(null, null, 1, 20, CancellationToken.None);

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor!.Total.Should().Be(2);
        resultado.Valor.Itens.Should().HaveCount(2);
        resultado.Valor.Pagina.Should().Be(1);
        resultado.Valor.TamanhoPagina.Should().Be(20);
    }

    [Fact]
    public async Task Listar_FiltrandoPorId_DeveRetornarUmUsuario()
    {
        var usuario = UsuarioMock("Alice", "alice@x.com", TipoUsuario.Usuario);
        var filtroId = usuario.Id;

        _repo.Setup(r => r.ListarAsync(filtroId, null, 1, 20, It.IsAny<CancellationToken>()))
             .ReturnsAsync((new List<Usuario> { usuario }, 1));

        var resultado = await Servico().ExecutarAsync(filtroId, null, 1, 20, CancellationToken.None);

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor!.Total.Should().Be(1);
        resultado.Valor.Itens.Should().ContainSingle(u => u.Email == "alice@x.com");
    }

    [Fact]
    public async Task Listar_FiltrandoPorRole_DeveRetornarSomenteAquelesTipo()
    {
        var admin = UsuarioMock("Bob", "bob@x.com", TipoUsuario.Administrador);

        _repo.Setup(r => r.ListarAsync(null, TipoUsuario.Administrador, 1, 20, It.IsAny<CancellationToken>()))
             .ReturnsAsync((new List<Usuario> { admin }, 1));

        var resultado = await Servico().ExecutarAsync(null, TipoUsuario.Administrador, 1, 20, CancellationToken.None);

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor!.Total.Should().Be(1);
        resultado.Valor.Itens.Should().OnlyContain(u => u.Tipo == "Administrador");
    }

    [Fact]
    public async Task Listar_ComPaginaInvalida_DeveCorrigirParaPagina1()
    {
        _repo.Setup(r => r.ListarAsync(null, null, 1, 20, It.IsAny<CancellationToken>()))
             .ReturnsAsync((new List<Usuario>(), 0));

        var resultado = await Servico().ExecutarAsync(null, null, -5, 20, CancellationToken.None);

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor!.Pagina.Should().Be(1);
    }

    [Fact]
    public async Task Listar_ComTamanhoPaginaExcessivo_DeveCorrigirPara20()
    {
        _repo.Setup(r => r.ListarAsync(null, null, 1, 20, It.IsAny<CancellationToken>()))
             .ReturnsAsync((new List<Usuario>(), 0));

        var resultado = await Servico().ExecutarAsync(null, null, 1, 500, CancellationToken.None);

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor!.TamanhoPagina.Should().Be(20);
    }

    [Fact]
    public async Task Listar_ResponseDeveConterTodosOsCamposDoCadastro()
    {
        var usuario = UsuarioMock("Alice", "alice@x.com", TipoUsuario.Usuario);

        _repo.Setup(r => r.ListarAsync(null, null, 1, 20, It.IsAny<CancellationToken>()))
             .ReturnsAsync((new List<Usuario> { usuario }, 1));

        var resultado = await Servico().ExecutarAsync(null, null, 1, 20, CancellationToken.None);

        var item = resultado.Valor!.Itens.Single();
        item.Id.Should().NotBeEmpty();
        item.Nome.Should().Be("Alice");
        item.Email.Should().Be("alice@x.com");
        item.Tipo.Should().Be("Usuario");
        item.DataCadastro.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Listar_FiltrandoPorIdInexistente_DeveRetornarNaoEncontrado()
    {
        var idInexistente = Guid.NewGuid();

        _repo.Setup(r => r.ListarAsync(idInexistente, null, 1, 20, It.IsAny<CancellationToken>()))
             .ReturnsAsync((new List<Usuario>(), 0));

        var resultado = await Servico().ExecutarAsync(idInexistente, null, 1, 20, CancellationToken.None);

        resultado.Sucesso.Should().BeFalse();
        resultado.Erro!.Codigo.Should().Be("NAO_ENCONTRADO");
        resultado.Erro.Mensagem.Should().Be("Usuário não encontrado");
    }
}
