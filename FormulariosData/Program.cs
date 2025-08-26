using FormulariosData;
using FormulariosData.Components;
using FormulariosData.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Configura��o da base de dados para usar uma "f�brica" de contextos
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Autentica��o com Google
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

// Adiciona os servi�os ao contentor.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Registra os servi�os do MudBlazor e Syncfusion
builder.Services.AddMudServices();

// Configura��o aprimorada do Blazor Server para evitar ObjectDisposedException
builder.Services.AddServerSideBlazor(options =>
{
    // Configura��es para desenvolvimento - ajuste conforme necess�rio
    options.DetailedErrors = builder.Environment.IsDevelopment();

    // Tempo de reten��o do circuito ap�s desconex�o
    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);

    // M�ximo de circuitos desconectados retidos
    options.DisconnectedCircuitMaxRetained = 100;

    // Timeout para reconex�o
    options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(1);

    // Tamanho m�ximo do buffer de mensagens
    options.MaxBufferedUnacknowledgedRenderBatches = 10;
});

// Registra o handler personalizado de circuitos
builder.Services.AddScoped<CircuitHandler, CustomCircuitHandler>();

// Adiciona autoriza��o
builder.Services.AddAuthorization();

// Adiciona estado de autentica��o em cascata
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

// Middleware de autentica��o e autoriza��o
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