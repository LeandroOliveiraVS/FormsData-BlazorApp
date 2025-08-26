using FormulariosData;
using FormulariosData.Components;
using FormulariosData.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuração da base de dados para usar uma "fábrica" de contextos
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Autenticação com Google
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Google:ClientId"];
    options.ClientSecret = builder.Configuration["Google:ClientSecret"];
});

// Adiciona os serviços ao contentor.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Registra os serviços do MudBlazor e Syncfusion
builder.Services.AddMudServices();

// Configuração aprimorada do Blazor Server para evitar ObjectDisposedException
builder.Services.AddServerSideBlazor(options =>
{
    // Configurações para desenvolvimento - ajuste conforme necessário
    options.DetailedErrors = builder.Environment.IsDevelopment();

    // Tempo de retenção do circuito após desconexão
    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);

    // Máximo de circuitos desconectados retidos
    options.DisconnectedCircuitMaxRetained = 100;

    // Timeout para reconexão
    options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(1);

    // Tamanho máximo do buffer de mensagens
    options.MaxBufferedUnacknowledgedRenderBatches = 10;
});

// Registra o handler personalizado de circuitos
builder.Services.AddScoped<CircuitHandler, CustomCircuitHandler>();

// Adiciona autorização
builder.Services.AddAuthorization();

// Adiciona estado de autenticação em cascata
builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

// Configura o pipeline de pedidos HTTP.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Middleware personalizado para tratamento de erros de API
app.UseApiErrorMiddleware();

app.UseAntiforgery();

// Middleware de autenticação e autorização
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Ativa a nossa API de recebimentos
app.MapRecebimentosApi();

// Ativa os nossos endpoints de login/logout
app.MapAuthEndpoints();

// Endpoint de health check para monitoramento
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }));

app.Run();