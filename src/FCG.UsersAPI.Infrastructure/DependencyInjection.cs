using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FCG.UsersAPI.Application.Comum.Interfaces;
using FCG.UsersAPI.Application.Identidade.Interfaces;
using FCG.UsersAPI.Domain.Identidade.Interfaces;
using FCG.UsersAPI.Infrastructure.Mensageria;
using FCG.UsersAPI.Infrastructure.Persistencia;
using FCG.UsersAPI.Infrastructure.Persistencia.Identidade.Repositorios;
using FCG.UsersAPI.Infrastructure.Persistencia.Seed;
using FCG.UsersAPI.Infrastructure.Seguranca;
namespace FCG.UsersAPI.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // EF Core — usa InMemory quando appsettings.Testing.json define UseInMemoryDatabase=true
        if (configuration.GetValue<bool>("UseInMemoryDatabase"))
            services.AddDbContext<ApplicationDbContext>(opts =>
                opts.UseInMemoryDatabase("fcg-users-tests"));
        else
            services.AddDbContext<ApplicationDbContext>(opts =>
                opts.UseNpgsql(configuration.GetConnectionString("Postgres")));

        // Unit of Work + Repositórios
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();

        // Segurança
        services.Configure<JwtConfig>(configuration.GetSection(JwtConfig.SectionName));
        services.Configure<SeedAdminConfig>(configuration.GetSection(SeedAdminConfig.SectionName));
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();

        // EventBus via MassTransit
        services.AddScoped<IEventBus, MassTransitEventBus>();

        // MassTransit + RabbitMQ (apenas publisher neste serviço)
        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(configuration["RabbitMQ:Host"] ?? "localhost",
                    configuration["RabbitMQ:VirtualHost"] ?? "/", h =>
                    {
                        h.Username(configuration["RabbitMQ:Username"] ?? "guest");
                        h.Password(configuration["RabbitMQ:Password"] ?? "guest");
                    });
                cfg.ConfigureEndpoints(ctx);
            });
        });

        // JWT Authentication
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtConfig>>((options, jwtOpts) =>
            {
                var jwt = jwtOpts.Value;
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true, ValidIssuer = jwt.Issuer,
                    ValidateAudience = true, ValidAudience = jwt.Audience,
                    ValidateLifetime = true, ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwt.Secret)) { KeyId = "fcg-signing-key" },
                    ClockSkew = TimeSpan.FromSeconds(30),
                    RoleClaimType = System.Security.Claims.ClaimTypes.Role
                };
            });

        // Seed
        services.AddScoped<SeedData>();

        return services;
    }
}
