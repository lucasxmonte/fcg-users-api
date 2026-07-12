using Microsoft.Extensions.Logging;
using FCG.Contracts.Events;
using FCG.UsersAPI.Application.Comum.Interfaces;
using FCG.UsersAPI.Application.Comum.Resultados;
using FCG.UsersAPI.Application.Identidade.DTOs;
using FCG.UsersAPI.Application.Identidade.Interfaces;
using FCG.UsersAPI.Domain.Comum;
using FCG.UsersAPI.Domain.Identidade.Entidades;
using FCG.UsersAPI.Domain.Identidade.Enums;
using FCG.UsersAPI.Domain.Identidade.Interfaces;
using FCG.UsersAPI.Domain.Identidade.ValueObjects;
namespace FCG.UsersAPI.Application.Identidade.Servicos;
public class CadastrarUsuarioService
{
    private readonly IUsuarioRepository _repo;
    private readonly IPasswordHasher _hasher;
    private readonly IUnitOfWork _uow;
    private readonly IEventBus _eventBus;
    private readonly ILogger<CadastrarUsuarioService> _logger;
    public CadastrarUsuarioService(IUsuarioRepository repo, IPasswordHasher hasher,
        IUnitOfWork uow, IEventBus eventBus, ILogger<CadastrarUsuarioService> logger)
    { _repo = repo; _hasher = hasher; _uow = uow; _eventBus = eventBus; _logger = logger; }

    public Task<Resultado<UsuarioResponse>> ExecutarAsync(RegistrarUsuarioRequest req, CancellationToken ct)
        => CriarAsync(req, TipoUsuario.Usuario, ct);

    public Task<Resultado<UsuarioResponse>> ExecutarComoAdminAsync(RegistrarUsuarioRequest req, CancellationToken ct)
        => CriarAsync(req, TipoUsuario.Administrador, ct);

    private async Task<Resultado<UsuarioResponse>> CriarAsync(
        RegistrarUsuarioRequest req, TipoUsuario tipo, CancellationToken ct)
    {
        try
        {
            var email = new Email(req.Email);
            _ = new Senha(req.Senha);
            if (await _repo.EmailExisteAsync(email.Valor, ct))
                return Resultado<UsuarioResponse>.Falha(Erro.Conflito("E-mail já cadastrado."));
            var senhaHash = _hasher.Gerar(req.Senha);
            var usuario = new Usuario(req.Nome, email, senhaHash, tipo);
            await _repo.AdicionarAsync(usuario, ct);
            await _uow.CommitAsync(ct);

            // Publica evento para NotificationsAPI enviar e-mail de boas-vindas
            await _eventBus.PublicarAsync(new UserCreatedEvent(
                usuario.Id, usuario.Nome, usuario.Email.Valor, usuario.DataCadastro), ct);

            _logger.LogInformation("Usuario registrado. Id={Id} Email={Email} Tipo={Tipo}",
                usuario.Id, usuario.Email.Valor, tipo);
            return Resultado<UsuarioResponse>.Ok(UsuarioResponse.From(usuario));
        }
        catch (DomainException ex)
        {
            return Resultado<UsuarioResponse>.Falha(Erro.Validacao(ex.Message));
        }
    }
}
