using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MissionLog.BlazorApp;
using MissionLog.BlazorApp.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Singleton auth state — shared across all pages
builder.Services.AddSingleton<AuthStateService>();

// ApiService — HttpClient base URL + AuthStateService for token attachment
builder.Services.AddScoped<ApiService>(sp =>
{
    var http      = new HttpClient { BaseAddress = new Uri("https://localhost:7100") };
    var authState = sp.GetRequiredService<AuthStateService>();
    return new ApiService(http, authState);
});

await builder.Build().RunAsync();
