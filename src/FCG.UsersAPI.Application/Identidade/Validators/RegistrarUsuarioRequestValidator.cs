using System.Text.RegularExpressions;
using FluentValidation;
using FCG.UsersAPI.Application.Identidade.DTOs;
namespace FCG.UsersAPI.Application.Identidade.Validators;
public class RegistrarUsuarioRequestValidator : AbstractValidator<RegistrarUsuarioRequest>
{
    private static readonly Regex ApenasLetrasEspacos = new(@"^[\p{L}\s]+$", RegexOptions.Compiled);
    private static readonly Regex PadraoEmail = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    private static readonly Regex CaracteresEmailValidos = new(@"^[a-zA-Z0-9._%+\-@]+$", RegexOptions.Compiled);
    public RegistrarUsuarioRequestValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().WithMessage("Campo nome obrigatório");
        RuleFor(x => x.Nome).Must(n => n.Trim().Contains(' ')).When(x => !string.IsNullOrWhiteSpace(x.Nome))
            .WithMessage("Campo nome deve conter nome e sobrenome");
        RuleFor(x => x.Nome).Must(n => ApenasLetrasEspacos.IsMatch(n.Trim()))
            .When(x => !string.IsNullOrWhiteSpace(x.Nome) && x.Nome.Trim().Contains(' '))
            .WithMessage("Campo nome deve conter somente letras");
        RuleFor(x => x.Email).NotEmpty().WithMessage("Campo e-mail obrigatório");
        RuleFor(x => x.Email).Must(e => CaracteresEmailValidos.IsMatch(e))
            .When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("Campo e-mail possui caracteres inválidos");
        RuleFor(x => x.Email).Must(e => PadraoEmail.IsMatch(e))
            .When(x => !string.IsNullOrWhiteSpace(x.Email) && CaracteresEmailValidos.IsMatch(x.Email))
            .WithMessage("E-mail invalido");
        RuleFor(x => x.Senha).NotEmpty().WithMessage("Campo senha obrigatório");
        RuleFor(x => x.Senha).MinimumLength(8).When(x => !string.IsNullOrEmpty(x.Senha))
            .WithMessage("A senha deve ter no mínimo 8 caracteres");
        RuleFor(x => x.Senha).Must(s => s.Any(char.IsUpper))
            .When(x => !string.IsNullOrEmpty(x.Senha) && x.Senha.Length >= 8)
            .WithMessage("A senha deve conter pelo menos uma letra maiúscula");
        RuleFor(x => x.Senha).Must(s => s.Any(char.IsLower))
            .When(x => !string.IsNullOrEmpty(x.Senha) && x.Senha.Length >= 8)
            .WithMessage("A senha deve conter pelo menos uma letra minúscula");
        RuleFor(x => x.Senha).Must(s => s.Any(char.IsDigit))
            .When(x => !string.IsNullOrEmpty(x.Senha) && x.Senha.Length >= 8)
            .WithMessage("A senha deve conter pelo menos um número");
        RuleFor(x => x.Senha).Must(s => s.Any(c => !char.IsLetterOrDigit(c)))
            .When(x => !string.IsNullOrEmpty(x.Senha) && x.Senha.Length >= 8)
            .WithMessage("A senha deve conter pelo menos um caractere especial");
    }
}
