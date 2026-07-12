using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using FCG.UsersAPI.Application.Comum.Interfaces;
using FCG.UsersAPI.Application.Identidade.Servicos;
using FCG.UsersAPI.Domain.Identidade.Entidades;
using FCG.UsersAPI.Domain.Identidade.Enums;
using FCG.UsersAPI.Domain.Identidade.Interfaces;
using FCG.UsersAPI.Domain.Identidade.ValueObjects;

namespace FCG.UsersAPI.Tests.Application;

public class PromoverUsuarioServiceTests
{
    private readonly Mock<IUsuarioRepository> _repo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<ILogger<PromoverUsuarioService>> _logger = new();

    private PromoverUsuarioService Servico() =>
        new(_repo.Object, _uow.Object, _logger.Object);

    private static Usuario UsuarioComum() =>
        new("João Silva", new Email("joao@x.com"), "hash", TipoUsuario.Usuario);

    [Fact]
    public async Task PromoverUsuario_IdInexistente_DeveRetornarNaoEncontrado()
    {
        _repo.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Usuario?)null);

        var resultado = await Servico().ExecutarAsync(Guid.NewGuid(), CancellationToken.None);

        resultado.Sucesso.Should().BeFalse();
        resultado.Erro!.Codigo.Should().Be("NAO_ENCONTRADO");
        resultado.Erro.Mensagem.Should().Contain("não encontrado");
    }

    [Fact]
    public async Task PromoverUsuario_UsuarioComum_DevePromoverParaAdministrador()
    {
        var usuario = UsuarioComum();

        _repo.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
             .ReturnsAsync(usuario);

        var resultado = await Servico().ExecutarAsync(usuario.Id, CancellationToken.None);

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor!.Tipo.Should().Be("Administrador");

        _repo.Verify(r => r.Atualizar(It.IsAny<Usuario>()), Times.Once);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PromoverUsuario_UsuarioJaAdministrador_DeveRetornarValidacao()
    {
        var admin = new Usuario("Admin Silva", new Email("admin@x.com"), "hash", TipoUsuario.Administrador);

        _repo.Setup(r => r.ObterPorIdAsync(admin.Id, It.IsAny<CancellationToken>()))
             .ReturnsAsync(admin);

        var resultado = await Servico().ExecutarAsync(admin.Id, CancellationToken.None);

        resultado.Sucesso.Should().BeFalse();
        resultado.Erro!.Codigo.Should().Be("VALIDACAO");
    }
}
