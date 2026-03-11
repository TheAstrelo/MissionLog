using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MissionLog.BlazorApp;
using MissionLog.BlazorApp.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Register AuthStateService as singleton (shared across pages)
builder.Services.AddSingleton<AuthStateService>();

// Register ApiService with HttpClient
builder.Services.AddScoped(sp =>
{
    var authState = sp.GetRequiredService<AuthStateService>();
    var client = new HttpClient { BaseAddress = new Uri("https://localhost:7100") };

    // Attach token if authenticated
    if (authState.Token != null)
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authState.Token);

    return client;
});

builder.Services.AddScoped<ApiService>();

await builder.Build().RunAsync();
