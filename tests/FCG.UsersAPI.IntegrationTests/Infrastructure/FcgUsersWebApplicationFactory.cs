using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FCG.UsersAPI.Infrastructure.Persistencia;

namespace FCG.UsersAPI.IntegrationTests.Infrastructure;

public class FcgUsersWebApplicationFactory : WebApplicationFactory<Program>
{
    static FcgUsersWebApplicationFactory()
    {
        // Garante que WebApplication.CreateBuilder() leia appsettings.Testing.json,
        // que ativa UseInMemoryDatabase=true e desativa o SeedAdmin.
        // Deve ser definido ANTES de qualquer instância do factory ser criada.
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove todos os IHostedService (MassTransit / RabbitMQ)
            // para evitar tentativas de conexão ao RabbitMQ durante os testes
            var hostedServices = services
                .Where(d => d.ServiceType == typeof(IHostedService))
                .ToList();
            foreach (var s in hostedServices)
                services.Remove(s);
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        var host = base.CreateHost(builder);

        // Garante que o schema InMemory está criado após o host estar pronto
        using var scope = host.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        ctx.Database.EnsureCreated();

        return host;
    }
}
