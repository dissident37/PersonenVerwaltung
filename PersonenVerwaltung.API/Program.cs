using Microsoft.EntityFrameworkCore;
using PersonenVerwaltung.Data;
using PersonenVerwaltung.Data.Repositories;

// Einstiegspunkt der API-Anwendung (Top-Level-Statements). Konfiguriert den
// DI-Container, erstellt die Anwendung und definiert die Middleware-Pipeline.
// Die Reihenfolge der Middleware bestimmt die Durchlaufreihenfolge jeder HTTP-Anfrage.

var builder = WebApplication.CreateBuilder(args);

// --- Dienste registrieren ---

// Verbindungszeichenfolge ermitteln: Vorrang hat die Umgebungsvariable (z. B. aus Docker),
// danach appsettings.json. Fehlt beides, wird der Start bewusst mit einer aussagekräftigen
// Ausnahme abgebrochen (Fail-Fast statt späterer, schwer deutbarer Folgefehler).
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DATABASE_URL ist nicht gesetzt.");

// DbContext mit dem PostgreSQL-Provider (Npgsql) registrieren.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Repository-Abstraktion an ihre Implementierung binden. Scoped (eine Instanz je
// HTTP-Anfrage), passend zur Lebensdauer des DbContext.
builder.Services.AddScoped<IPersonRepository, PersonRepository>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
// OpenAPI-/Swagger-Dokumentation erzeugen (erreichbar unter /swagger).
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "PersonenVerwaltung API", Version = "v1" });
});

// CORS: Da UI und API unter unterschiedlichen Ursprüngen laufen, werden hier
// herkunftsübergreifende Anfragen erlaubt. Die offene Richtlinie ist für Demo und
// Entwicklung gedacht und sollte für den Produktivbetrieb eingeschränkt werden.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// --- Anwendung erstellen ---
var app = builder.Build();

// --- Middleware-Pipeline ---

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();
app.MapControllers();

app.Run();
