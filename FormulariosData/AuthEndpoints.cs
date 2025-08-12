using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        // Endpoint para iniciar login com Google
        app.MapGet("/login-google", () =>
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = "/login-callback" // Redireciona para callback após login
            };

            properties.Parameters.Add("prompt", "select_account");

            return Results.Challenge(properties, [GoogleDefaults.AuthenticationScheme]);
        });

        // Callback após login do Google
        app.MapGet("/login-callback", async (HttpContext context) =>
        {
            var authResult = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (authResult.Succeeded)
            {
                Console.WriteLine($"Login bem-sucedido para: {authResult.Principal.Identity?.Name}");
                context.Response.Redirect("/"); // Redireciona para a página principal
            }
            else
            {
                Console.WriteLine("Falha no login");
                context.Response.Redirect("/login?error=failed");
            }
        });

        // Endpoint de logout
        app.MapPost("/logout", async (HttpContext context) =>
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await context.SignOutAsync(GoogleDefaults.AuthenticationScheme);

            Console.WriteLine($"Logout realizado para: {context.User.Identity?.Name}");

            // Limpa cookies de autenticação
            context.Response.Cookies.Delete(".AspNetCore.Cookies");

            return Results.Redirect("/login");
        });

        // Endpoint GET para logout (para links diretos)
        app.MapGet("/logout", async (HttpContext context) =>
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            //await context.SignOutAsync(GoogleDefaults.AuthenticationScheme);

            Console.WriteLine($"Logout realizado para: {context.User.Identity?.Name}");

            // Limpa cookies de autenticação
            context.Response.Cookies.Delete(".AspNetCore.Cookies");

            context.Response.Redirect("/login");
        });

        // Endpoint para verificar status de autenticação
        app.MapGet("/auth/status", (HttpContext context) =>
        {
            var isAuthenticated = context.User.Identity?.IsAuthenticated ?? false;
            var userName = context.User.Identity?.Name ?? "";

            return Results.Json(new
            {
                isAuthenticated,
                userName,
                claims = context.User.Claims.Select(c => new { c.Type, c.Value }).ToList()
            });
        });
    }
}
