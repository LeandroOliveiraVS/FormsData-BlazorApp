namespace FormulariosData
{
    public class ApiErrorMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiErrorMiddleware> _logger;

        public ApiErrorMiddleware(RequestDelegate next, ILogger<ApiErrorMiddleware> logger)
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
                _logger.LogError(ex, "Erro não tratado na requisição {RequestPath}", context.Request.Path);

                // Se for uma requisição para API, retorna JSON
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    await HandleApiExceptionAsync(context, ex);
                }
                else
                {
                    throw; // Re-throw para outras páginas
                }
            }
        }

        private static async Task HandleApiExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = new
                {
                    message = "Ocorreu um erro interno no servidor",
                    detail = exception.Message,
                    timestamp = DateTime.UtcNow
                }
            };

            context.Response.StatusCode = exception switch
            {
                UnauthorizedAccessException => 401,
                ArgumentException => 400,
                KeyNotFoundException => 404,
                OperationCanceledException => 499,
                _ => 500
            };

            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(jsonResponse);
        }
    }

    public static class ApiErrorMiddlewareExtensions
    {
        public static IApplicationBuilder UseApiErrorMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiErrorMiddleware>();
        }
    }
}
