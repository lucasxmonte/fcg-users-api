using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using FCG.UsersAPI.Application.Identidade.DTOs;
using FCG.UsersAPI.Application.Identidade.Interfaces;
using FCG.UsersAPI.Application.Identidade.Servicos;
using FCG.UsersAPI.Domain.Identidade.Entidades;
using FCG.UsersAPI.Domain.Identidade.Enums;
using FCG.UsersAPI.Domain.Identidade.Interfaces;
using FCG.UsersAPI.Domain.Identidade.ValueObjects;

namespace FCG.UsersAPI.Tests.Application;

public class AutenticarUsuarioServiceTests
{
    private readonly Mock<IUsuarioRepository> _repo = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IJwtTokenService> _jwt = new();
    private readonly Mock<IRefreshTokenService> _refresh = new();
    private readonly Mock<ILogger<AutenticarUsuarioService>> _logger = new();

    private AutenticarUsuarioService Servico() =>
        new(_repo.Object, _hasher.Object, _jwt.Object, _refresh.Object, _logger.Object);

    private static Usuario UsuarioMock() =>
        new("Maria", new Email("maria@x.com"), "hash-stored", TipoUsuario.Usuario);

    [Fact]
    public async Task Login_ComCredenciaisValidas_DeveRetornarSucesso()
    {
        _repo.Setup(r => r.ObterPorEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(UsuarioMock());
        _hasher.Setup(h => h.Verificar(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        _jwt.Setup(j => j.GerarAccessToken(It.IsAny<Usuario>())).Returns("jwt-token");
        _jwt.Setup(j => j.ObterDuracaoEmSegundos()).Returns(900);
        _refresh.Setup(r => r.GerarAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("refresh-abc");

        var resultado = await Servico().ExecutarAsync(
            new LoginRequest("maria@x.com", "Senha@123"),
            CancellationToken.None);

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor!.AccessToken.Should().Be("jwt-token");
        resultado.Valor.RefreshToken.Should().Be("refresh-abc");
        resultado.Valor.ExpiresIn.Should().Be(900);
        resultado.Valor.TokenType.Should().Be("Bearer");
    }

    [Fact]
    public async Task Login_ComEmailInexistente_DeveRetornarNaoAutenticado()
    {
        _repo.Setup(r => r.ObterPorEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Usuario?)null);

        var resultado = await Servico().ExecutarAsync(
            new LoginRequest("naoexiste@x.com", "Senha@123"),
            CancellationToken.None);

        resultado.Sucesso.Should().BeFalse();
        resultado.Erro!.Codigo.Should().Be("NAO_AUTENTICADO");
    }

    [Fact]
    public async Task Login_ComSenhaErrada_DeveRetornarNaoAutenticado()
    {
        _repo.Setup(r => r.ObterPorEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(UsuarioMock());
        _hasher.Setup(h => h.Verificar(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var resultado = await Servico().ExecutarAsync(
            new LoginRequest("maria@x.com", "errada"),
            CancellationToken.None);

        resultado.Sucesso.Should().BeFalse();
        resultado.Erro!.Codigo.Should().Be("NAO_AUTENTICADO");
    }

    [Fact]
    public async Task Refresh_ComTokenValido_DeveRetornarNovosTokens()
    {
        var usuario = UsuarioMock();
        _refresh.Setup(r => r.ValidarERotacionarAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(usuario.Id);
        _repo.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
             .ReturnsAsync(usuario);
        _jwt.Setup(j => j.GerarAccessToken(It.IsAny<Usuario>())).Returns("novo-jwt");
        _jwt.Setup(j => j.ObterDuracaoEmSegundos()).Returns(900);
        _refresh.Setup(r => r.GerarAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("novo-refresh");

        var resultado = await Servico().RenovarAsync("token-valido", CancellationToken.None);

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor!.AccessToken.Should().Be("novo-jwt");
        resultado.Valor.RefreshToken.Should().Be("novo-refresh");
    }

    [Fact]
    public async Task Refresh_ComTokenIncorreto_DeveRetornarNaoAutenticado()
    {
        _refresh.Setup(r => r.ValidarERotacionarAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid?)null);

        var resultado = await Servico().RenovarAsync("token-ruim", CancellationToken.None);

        resultado.Sucesso.Should().BeFalse();
        resultado.Erro!.Codigo.Should().Be("NAO_AUTENTICADO");
        resultado.Erro.Mensagem.Should().Be("Refresh token incorreto ou invalido");
    }

    [Fact]
    public async Task Refresh_SemToken_DeveRetornarNaoAutenticado()
    {
        _refresh.Setup(r => r.ValidarERotacionarAsync(string.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid?)null);

        var resultado = await Servico().RenovarAsync(string.Empty, CancellationToken.None);

        resultado.Sucesso.Should().BeFalse();
        resultado.Erro!.Codigo.Should().Be("NAO_AUTENTICADO");
        resultado.Erro.Mensagem.Should().Be("Refresh token incorreto ou invalido");
    }

    [Fact]
    public async Task Logout_SemToken_DeveRetornarValidacao()
    {
        var resultado = await Servico().LogoutAsync(string.Empty, CancellationToken.None);

        resultado.Sucesso.Should().BeFalse();
        resultado.Erro!.Codigo.Should().Be("VALIDACAO");
        resultado.Erro.Mensagem.Should().Be("Não foi possível realizar o logout devido a ausência do refresh token");
    }

    [Fact]
    public async Task Logout_ComTokenIncorreto_DeveRetornarValidacao()
    {
        _refresh.Setup(r => r.RevogarComValidacaoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

        var resultado = await Servico().LogoutAsync("token-invalido", CancellationToken.None);

        resultado.Sucesso.Should().BeFalse();
        resultado.Erro!.Codigo.Should().Be("VALIDACAO");
        resultado.Erro.Mensagem.Should().Be("Não foi possível realizar o logout, refresh token incorreto/invalido");
    }
}
