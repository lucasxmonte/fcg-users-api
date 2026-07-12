using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using FCG.UsersAPI.Application.Comum.Interfaces;
using FCG.UsersAPI.Application.Identidade.DTOs;
using FCG.UsersAPI.Application.Identidade.Interfaces;
using FCG.UsersAPI.Application.Identidade.Servicos;
using FCG.UsersAPI.Domain.Identidade.Entidades;
using FCG.UsersAPI.Domain.Identidade.Interfaces;

namespace FCG.UsersAPI.Tests.Application;

public class CadastrarUsuarioServiceTests
{
    private readonly Mock<IUsuarioRepository> _repo = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IEventBus> _eventBus = new();
    private readonly Mock<ILogger<CadastrarUsuarioService>> _logger = new();

    private CadastrarUsuarioService Servico() =>
        new(_repo.Object, _hasher.Object, _uow.Object, _eventBus.Object, _logger.Object);

    [Fact]
    public async Task Executar_QuandoEmailJaExiste_DeveRetornarConflito()
    {
        _repo.Setup(r => r.EmailExisteAsync("ja@existe.com", It.IsAny<CancellationToken>()))
             .ReturnsAsync(true);

        var r = await Servico().ExecutarAsync(
            new RegistrarUsuarioRequest("Maria", "ja@existe.com", "Senha@123"),
            CancellationToken.None);

        r.Sucesso.Should().BeFalse();
        r.Erro!.Codigo.Should().Be("CONFLITO");
    }

    [Fact]
    public async Task Executar_ComDadosValidos_DeveCriarEPersistir()
    {
        _repo.Setup(r => r.EmailExisteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(false);
        _hasher.Setup(h => h.Gerar(It.IsAny<string>())).Returns("hash-bcrypt");

        var r = await Servico().ExecutarAsync(
            new RegistrarUsuarioRequest("Maria", "maria@x.com", "Senha@123"),
            CancellationToken.None);

        r.Sucesso.Should().BeTrue();
        r.Valor!.Email.Should().Be("maria@x.com");
        r.Valor.Tipo.Should().Be("Usuario");

        _repo.Verify(repo => repo.AdicionarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Executar_ComEmailInvalido_DeveRetornarValidacao()
    {
        _repo.Setup(r => r.EmailExisteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(false);

        var r = await Servico().ExecutarAsync(
            new RegistrarUsuarioRequest("Maria", "naoEhEmail", "Senha@123"),
            CancellationToken.None);

        r.Sucesso.Should().BeFalse();
        r.Erro!.Codigo.Should().Be("VALIDACAO");
    }

    [Fact]
    public async Task Executar_ComSenhaFraca_DeveRetornarValidacao()
    {
        _repo.Setup(r => r.EmailExisteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(false);

        var r = await Servico().ExecutarAsync(
            new RegistrarUsuarioRequest("Maria", "maria@x.com", "fraca"),
            CancellationToken.None);

        r.Sucesso.Should().BeFalse();
        r.Erro!.Codigo.Should().Be("VALIDACAO");
    }

    [Fact]
    public async Task ExecutarComoAdmin_ComDadosValidos_DeveCriarComoAdministrador()
    {
        _repo.Setup(r => r.EmailExisteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(false);
        _hasher.Setup(h => h.Gerar(It.IsAny<string>())).Returns("hash");

        var r = await Servico().ExecutarComoAdminAsync(
            new RegistrarUsuarioRequest("Joao Admin", "joao@admin.com", "Senha@123"),
            CancellationToken.None);

        r.Sucesso.Should().BeTrue();
        r.Valor!.Tipo.Should().Be("Administrador");
    }
}
