using PersonenVerwaltung.UI;
using PersonenVerwaltung.UI.Services;

// Das ist der Startpunkt der Benutzeroberfläche (der Web-Seiten, die der Nutzer sieht).
// Hier werden die Helfer angemeldet, der Server gebaut und gestartet.

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();          // schaltet die Web-Seiten ein.
builder.Services.AddServerSideBlazor();    // schaltet Blazor ein (damit reagieren die Seiten ohne Neuladen).

// Adresse der API besorgen, damit die Oberfläche weiß, wo sie ihre Daten holen kann.
// Zuerst aus den Umgebungseinstellungen, sonst aus appsettings.json, sonst die lokale Standard-Adresse.
var apiUrl = Environment.GetEnvironmentVariable("API_URL")
    ?? builder.Configuration["ApiUrl"]
    ?? "http://localhost:5000";

// Den Helfer anmelden, der die Anfragen an die API verschickt (PersonApiService),
// und ihm die oben besorgte API-Adresse fest mitgeben.
builder.Services.AddHttpClient<PersonApiService>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

var app = builder.Build();

// Außerhalb der Entwicklung zeigen wir bei Fehlern eine eigene Fehlerseite,
// statt dem Nutzer technische Details zu zeigen.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();          // liefert feste Dateien aus (z.B. CSS, Bilder).
app.UseRouting();              // sorgt dafür, dass jede Adresse zur richtigen Seite führt.
app.MapBlazorHub();            // baut die ständige Verbindung zwischen Browser und Server auf.
app.MapFallbackToPage("/_Host"); // alles, was sonst nirgends passt, geht an die Startseite.

app.Run();                     // startet den Server.
