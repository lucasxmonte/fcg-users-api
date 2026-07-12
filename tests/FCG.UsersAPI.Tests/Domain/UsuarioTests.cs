using FluentAssertions;
using FCG.UsersAPI.Domain.Comum;
using FCG.UsersAPI.Domain.Identidade.Entidades;
using FCG.UsersAPI.Domain.Identidade.Enums;
using FCG.UsersAPI.Domain.Identidade.ValueObjects;

namespace FCG.UsersAPI.Tests.Domain;

public class UsuarioTests
{
    [Fact]
    public void Criar_ComDadosValidos_DeveCriarUsuario()
    {
        var u = new Usuario("João Silva", new Email("joao@x.com"), "hash", TipoUsuario.Usuario);

        u.Nome.Should().Be("João Silva");
        u.Email.Valor.Should().Be("joao@x.com");
        u.TipoUsuario.Should().Be(TipoUsuario.Usuario);
        u.Id.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("A")]
    public void Criar_ComNomeInvalido_DeveLancarDomainException(string nome)
    {
        Action act = () => new Usuario(nome, new Email("x@x.com"), "hash", TipoUsuario.Usuario);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Criar_ComSenhaHashVazia_DeveLancarDomainException()
    {
        Action act = () => new Usuario("Maria", new Email("m@x.com"), string.Empty, TipoUsuario.Usuario);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void PromoverParaAdministrador_DeveAlterarTipo()
    {
        var u = new Usuario("Maria", new Email("m@x.com"), "hash", TipoUsuario.Usuario);
        u.PromoverParaAdministrador();
        u.TipoUsuario.Should().Be(TipoUsuario.Administrador);
    }

    [Fact]
    public void PromoverParaAdministrador_QuandoJaAdmin_DeveLancarDomainException()
    {
        var u = new Usuario("Admin", new Email("a@x.com"), "hash", TipoUsuario.Administrador);
        Action act = () => u.PromoverParaAdministrador();
        act.Should().Throw<DomainException>();
    }
}
