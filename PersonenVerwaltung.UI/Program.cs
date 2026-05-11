using PersonenVerwaltung.UI;
using PersonenVerwaltung.UI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// API-URL aus Umgebungsvariable
var apiUrl = Environment.GetEnvironmentVariable("API_URL")
    ?? builder.Configuration["ApiUrl"]
    ?? "http://localhost:5000";

builder.Services.AddHttpClient<PersonApiService>(client =>
{
    client.BaseAddress = new Uri(apiUrl);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
