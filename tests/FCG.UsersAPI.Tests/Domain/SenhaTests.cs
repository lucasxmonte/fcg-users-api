using FluentAssertions;
using FCG.UsersAPI.Domain.Comum;
using FCG.UsersAPI.Domain.Identidade.ValueObjects;

namespace FCG.UsersAPI.Tests.Domain;

public class SenhaTests
{
    [Fact]
    public void Criar_ComSenhaVazia_DeveLancarDomainException()
    {
        Action act = () => new Senha(string.Empty);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Criar_ComSenhaMenorQueMinimo_DeveLancarDomainException()
    {
        Action act = () => new Senha("Ab1!");
        act.Should().Throw<DomainException>()
            .WithMessage($"*{Senha.TamanhoMinimo}*");
    }

    [Fact]
    public void Criar_SemLetras_DeveLancarDomainException()
    {
        Action act = () => new Senha("12345678!");
        act.Should().Throw<DomainException>().WithMessage("*letra*");
    }

    [Fact]
    public void Criar_SemNumeros_DeveLancarDomainException()
    {
        Action act = () => new Senha("Abcdefg!");
        act.Should().Throw<DomainException>().WithMessage("*número*");
    }

    [Fact]
    public void Criar_SemCaracteresEspeciais_DeveLancarDomainException()
    {
        Action act = () => new Senha("Abcdefg1");
        act.Should().Throw<DomainException>().WithMessage("*especial*");
    }

    [Fact]
    public void Criar_ComSenhaValida_DeveCriar()
    {
        var s = new Senha("Senha@123");
        s.Valor.Should().Be("Senha@123");
    }
}
