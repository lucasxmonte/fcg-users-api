namespace FCG.UsersAPI.Application.Comum.Interfaces;
/// <summary>Abstração de publicação de eventos de domínio. Implementada via MassTransit na Infrastructure.</summary>
public interface IEventBus
{
    Task PublicarAsync<T>(T evento, CancellationToken ct) where T : class;
}
