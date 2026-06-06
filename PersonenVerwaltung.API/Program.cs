using Microsoft.EntityFrameworkCore;     // für UseNpgsql (PostgreSQL-Anbindung an EF Core).
using PersonenVerwaltung.Data;
using PersonenVerwaltung.Data.Repositories;

// WAS IST DAS: Der Startpunkt ("Einstiegspunkt") der gesamten API-Anwendung.
// WARUM BRAUCHEN WIR DAS: Hier wird der Webserver aufgebaut, alle benötigten Dienste registriert
//                         (Datenbank, Repository, Swagger, CORS) und am Ende gestartet.
// IM VORSTELLUNGSGESPRÄCH SAGEN:
//   "Das ist das 'Top-Level-Statements'-Programm von .NET – es gibt keine sichtbare Main-Methode, der Compiler
//    erzeugt sie. Erst konfiguriere ich den DI-Container (builder.Services.Add...), dann baue ich die App
//    (builder.Build()) und definiere die Middleware-Pipeline (app.Use...). Reihenfolge der Middleware ist wichtig:
//    Jede Anfrage läuft der Reihe nach durch diese Schritte."

// builder = Baukasten für die Anwendung. "args" = Kommandozeilen-Argumente.
var builder = WebApplication.CreateBuilder(args);

// --- 1) Dienste registrieren (Dependency-Injection-Container befüllen) ---

// Verbindungszeichenfolge (Connection String) bestimmen.
// "??" = Null-Coalescing: nimm den linken Wert; ist er null, nimm den rechten.
// Reihenfolge: zuerst Umgebungsvariable DATABASE_URL (z.B. aus Docker), sonst appsettings.json,
// und wenn beides fehlt -> Ausnahme werfen (lieber sofort klar scheitern als später kryptisch).
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DATABASE_URL ist nicht gesetzt.");

// Den AppDbContext im DI-Container registrieren und EF Core sagen: nutze PostgreSQL (Npgsql) mit diesem String.
// Damit kann der DbContext automatisch in Konstruktoren (z.B. PersonRepository) eingespritzt werden.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Repository registrieren: "wenn jemand ein IPersonRepository braucht, gib ihm ein PersonRepository".
// AddScoped = eine Instanz pro HTTP-Anfrage (Lebensdauer "scoped"). Passend, weil der DbContext auch scoped ist.
builder.Services.AddScoped<IPersonRepository, PersonRepository>();

builder.Services.AddControllers();         // aktiviert die Controller (unser PersonsController).
builder.Services.AddEndpointsApiExplorer();// liefert Metadaten der Endpunkte – nötig für Swagger.
// Swagger = automatisch erzeugte API-Doku + Test-Oberfläche im Browser (unter /swagger).
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "PersonenVerwaltung API", Version = "v1" });
});

// CORS = Cross-Origin Resource Sharing. Regelt, welche fremden Web-Adressen die API aufrufen dürfen.
// Browser blockieren standardmäßig Anfragen von anderen Ursprüngen (Origin = Domain + Port).
// Hier: alles erlaubt (für Demo/Entwicklung bequem; in Produktion sollte man das einschränken).
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// --- 2) Die Anwendung bauen ---
var app = builder.Build();

// --- 3) Middleware-Pipeline definieren (Reihenfolge = Durchlaufreihenfolge jeder Anfrage) ---
// Middleware = "Bearbeitungsschritte", die jede HTTP-Anfrage nacheinander durchläuft.

app.UseSwagger();      // stellt die Swagger-Beschreibung (JSON) bereit.
app.UseSwaggerUI();    // stellt die interaktive Swagger-Weboberfläche bereit.

app.UseCors();         // wendet die oben definierte CORS-Regel an.
app.MapControllers();  // verbindet eingehende URLs mit den passenden Controller-Methoden (Routing).

app.Run();             // startet den Webserver und blockiert hier, solange die App läuft.
