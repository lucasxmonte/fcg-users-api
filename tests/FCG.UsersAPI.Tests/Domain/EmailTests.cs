using FluentAssertions;
using FCG.UsersAPI.Domain.Comum;
using FCG.UsersAPI.Domain.Identidade.ValueObjects;

namespace FCG.UsersAPI.Tests.Domain;

public class EmailTests
{
    [Fact]
    public void Criar_ComEmailVazio_DeveLancarDomainException()
    {
        Action act = () => new Email(string.Empty);
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("semArroba")]
    [InlineData("@semlocal.com")]
    [InlineData("sem@dominio")]
    [InlineData("sem.arroba.tudo")]
    public void Criar_ComEmailFormatoInvalido_DeveLancarDomainException(string emailInvalido)
    {
        Action act = () => new Email(emailInvalido);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Criar_ComEmailValido_DeveNormalizar()
    {
        var email = new Email("  Usuario@Example.COM  ");
        email.Valor.Should().Be("usuario@example.com");
    }

    [Fact]
    public void Igualdade_ComMesmoValorNormalizado_DeveSerIgual()
    {
        var e1 = new Email("maria@x.com");
        var e2 = new Email("MARIA@X.COM");
        e1.Should().Be(e2);
    }

    [Fact]
    public void ToString_DeveRetornarValorNormalizado()
    {
        var email = new Email("Teste@X.com");
        email.ToString().Should().Be("teste@x.com");
    }
}
