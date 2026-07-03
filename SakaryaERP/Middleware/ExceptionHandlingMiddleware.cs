namespace SakaryaERP.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "İşlenmeyen hata oluştu. Path: {Path}", context.Request.Path);

            if (!context.Response.HasStarted)
            {
                context.Response.Redirect("/Home/Error");
            }
        }
    }
}
