using Retail.UI.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Optionally keep factory for named clients too
builder.Services.AddHttpClient();

// Register HttpClient (default for @inject HttpClient)
builder.Services.AddScoped<HttpClient>(sp =>
{
    var navigationManager = sp.GetRequiredService<NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(navigationManager.BaseUri) };
});

builder.Services.AddRazorPages();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

try
{
    logger.LogInformation("Starting Retail UI application");

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        logger.LogInformation("Configuring production error handling");
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        app.UseHsts();
    }
    else
    {
        logger.LogInformation("Running in Development mode");
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseAntiforgery();

    app.MapRazorPages();
    app.MapBlazorHub();
    app.MapFallbackToPage("/_Host");

    logger.LogInformation("Retail UI application started successfully");

    app.Run();
}
catch (Exception ex)
{
    logger.LogError(ex, "Error starting Retail UI application");
    throw;
}
