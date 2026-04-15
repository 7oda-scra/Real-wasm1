using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using RealDesign2.Client;
using RealDesign2.Client.Interfaces;
using RealDesign2.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var configuredApiBaseAddress = builder.Configuration["ApiBaseUrl"];
var apiBaseAddress = string.IsNullOrWhiteSpace(configuredApiBaseAddress)
    ? new Uri(builder.HostEnvironment.BaseAddress)
    : new Uri(new Uri(builder.HostEnvironment.BaseAddress), configuredApiBaseAddress);

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = apiBaseAddress });

builder.Services.AddMudServices();
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddScoped<IAuthSessionStore, InMemoryAuthSessionStore>();
builder.Services.AddScoped<IAuthService, ApiAuthService>();
builder.Services.AddScoped<IModuleService, ApiModuleService>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

await builder.Build().RunAsync();
