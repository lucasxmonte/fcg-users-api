using FluentValidation;
using Serilog;
using FCG.UsersAPI.API.Endpoints;
using FCG.UsersAPI.API.Middlewares;
using FCG.UsersAPI.Application.Identidade.DTOs;
using FCG.UsersAPI.Application.Identidade.Servicos;
using FCG.UsersAPI.Application.Identidade.Validators;
using FCG.UsersAPI.Infrastructure;
using FCG.UsersAPI.Infrastructure.Persistencia;
using FCG.UsersAPI.Infrastructure.Persistencia.Seed;
using Microsoft.EntityFrameworkCore;

// Em Testing, NÃO inicializa o UseSerilog para evitar "The logger is already frozen"
// (xUnit cria uma WebApplicationFactory por classe de teste — todas rodam no mesmo processo).
var isTestEnv = string.Equals(
    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
    "Testing",
    StringComparison.OrdinalIgnoreCase);

var builder = WebApplication.CreateBuilder(args);

if (!isTestEnv)
{
    builder.Host.UseSerilog((ctx, _, lc) =>
        lc.ReadFrom.Configuration(ctx.Configuration).Enrich.FromLogContext()
          .WriteTo.Console());
}

// Se o ambiente for "Testing", força InMemory independente do appsettings.Testing.json
// (duplo check: garante InMemory mesmo se o arquivo não for encontrado a tempo)
if (builder.Environment.IsEnvironment("Testing"))
    builder.Configuration["UseInMemoryDatabase"] = "true";

builder.Services.AddInfrastructure(builder.Configuration);

// Application services
builder.Services.AddScoped<AutenticarUsuarioService>();
builder.Services.AddScoped<CadastrarUsuarioService>();
builder.Services.AddScoped<ListarUsuariosService>();
builder.Services.AddScoped<PromoverUsuarioService>();
builder.Services.AddScoped<AtualizarUsuarioService>();
builder.Services.AddScoped<RemoverUsuarioService>();

// Validators
builder.Services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
builder.Services.AddScoped<IValidator<RegistrarUsuarioRequest>, RegistrarUsuarioRequestValidator>();

// Authorization
builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("UsuarioAutenticado", p => p.RequireAuthenticatedUser());
    o.AddPolicy("AdminOnly", p => p.RequireAuthenticatedUser().RequireRole("Administrador"));
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "FCG Users API",
        Version = "v1",
        Description =
            "API de identidade da plataforma FCG. Gerencia autenticação (JWT + refresh token) e cadastro/administração de usuários.\n\n" +
            "### Autenticação\n" +
            "Use `POST /api/auth/login` para obter um access token e inclua-o no cabeçalho `Authorization: Bearer {token}`.\n\n" +
            "### Respostas de erro padrão\n" +
            "| Schema | Quando é retornado |\n" +
            "|--------|--------------------|\n" +
            "| `ErroResponse` | Erro de negócio com mensagem única (401, 403, 404, 409) |\n" +
            "| `ValidacaoErroResponse` | Falha de validação de entrada com lista de mensagens (400) |"
    });
    c.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Informe apenas o token JWT obtido em `POST /api/auth/login`, sem o prefixo \"Bearer\". O Swagger adiciona o prefixo automaticamente."
    });
    c.AddSecurityRequirement(new() { {
        new() { Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" } },
        Array.Empty<string>()
    } });
    // Garante que ErroResponse e ValidacaoErroResponse apareçam no schema global
    c.UseAllOfToExtendReferenceSchemas();
});

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

// Swagger disponível em todos os ambientes exceto Production
if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FCG Users API v1");
        c.DocumentTitle = "FCG Users API";
        c.DefaultModelsExpandDepth(2);   // expande schemas de erro por padrão
        c.DisplayRequestDuration();
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "users-api", timestamp = DateTime.UtcNow }))
   .WithTags("Sistema").ExcludeFromDescription();

app.MapAutenticacaoEndpoints();
app.MapUsuarioEndpoints();

// Migrations + Seed — só executa quando o banco é relacional (Postgres).
// InMemory retorna IsRelational() == false, portanto é ignorado aqui.
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (db.Database.IsRelational())
    {
        try
        {
            await db.Database.MigrateAsync();
            var seed = scope.ServiceProvider.GetRequiredService<SeedData>();
            await seed.ExecutarAsync();
        }
        catch (Exception ex)
        {
            if (!isTestEnv) Log.Fatal(ex, "❌ Falha ao aplicar migrations. Postgres está rodando? (Docker deve estar ativo)");
            Console.Error.WriteLine($"FATAL: Falha ao aplicar migrations: {ex.Message}");
            throw;
        }
    }
}

if (!isTestEnv) Log.Information("FCG Users API iniciando...");
await app.RunAsync();

public partial class Program { }
