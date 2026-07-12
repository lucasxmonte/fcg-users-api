using FluentValidation;
namespace FCG.UsersAPI.API.Comum;
public static class ValidationExtensions
{
    public static async Task<IResult?> ValidarOuFalharAsync<T>(
        this IValidator<T> validator, T instance, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(instance, ct);
        if (result.IsValid) return null;
        var erros = result.Errors.Select(e => e.ErrorMessage).ToList();
        return Results.BadRequest(new ValidacaoErroResponse(erros));
    }
}
