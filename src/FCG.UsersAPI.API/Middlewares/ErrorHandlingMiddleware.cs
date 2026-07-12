using System.Text.Json;
using FCG.UsersAPI.Domain.Comum;
namespace FCG.UsersAPI.API.Middlewares;
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    { _next = next; _logger = logger; }
    public async Task InvokeAsync(HttpContext ctx)
    {
        try { await _next(ctx); }
        catch (DomainException ex)
        {
            _logger.LogWarning("DomainException: {Message}", ex.Message);
            ctx.Response.StatusCode = 400;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsync(JsonSerializer.Serialize(new { erro = ex.Message }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro não tratado.");
            ctx.Response.StatusCode = 500;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsync(JsonSerializer.Serialize(new { erro = "Erro interno." }));
        }
    }
}
