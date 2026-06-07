using PersonenVerwaltung.UI;
using PersonenVerwaltung.UI.Services;

// Einstiegspunkt der Blazor-Server-Anwendung (UI-Schicht). Registriert die Dienste,
// erstellt die Anwendung und konfiguriert die Middleware-Pipeline.

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Basis-Adresse der API ermitteln: Vorrang hat die Umgebungsvariable, danach
// appsettings.json, schließlich ein Standardwert für die lokale Entwicklung.
var apiUrl = Environment.GetEnvironmentVariable("API_URL")
    ?? builder.Configuration["ApiUrl"]
    ?? "http://localhost:5000";

// Typisierten HttpClient für den PersonApiService registrieren. Die zentrale
// Konfiguration der Basis-Adresse hält die UI frei von URL-Details.
builder.Services.AddHttpClient<PersonApiService>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

var app = builder.Build();

// Außerhalb der Entwicklung Fehler über eine eigene Seite behandeln statt detaillierte
// Ausnahmen offenzulegen.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
// SignalR-Endpunkt für die Blazor-Server-Verbindung.
app.MapBlazorHub();
// Alle nicht zugeordneten Anfragen an die Host-Seite leiten (SPA-Fallback).
app.MapFallbackToPage("/_Host");

app.Run();
