using FluentValidation;
using FCG.UsersAPI.Application.Identidade.DTOs;
namespace FCG.UsersAPI.Application.Identidade.Validators;
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage("E-mail obrigatório")
            .EmailAddress().WithMessage("E-mail inválido");
        RuleFor(x => x.Senha).NotEmpty().WithMessage("Senha obrigatória");
    }
}
