using Microsoft.EntityFrameworkCore;
using PersonenVerwaltung.Data;
using PersonenVerwaltung.Data.Repositories;

// Das ist der Startpunkt der API (des Web-Dienstes).
// Hier wird der Webserver eingerichtet: erst werden alle Helfer angemeldet (Datenbank,
// Repository, Doku-Seite, Zugriffsregeln), dann wird der Server gebaut und gestartet.
// Die Reihenfolge weiter unten ist wichtig: Jede Anfrage läuft die Schritte der Reihe nach durch.

var builder = WebApplication.CreateBuilder(args);

// --- Helfer anmelden ---

// Adresse der Datenbank besorgen.
// Zuerst aus den Umgebungseinstellungen (z.B. von Docker), sonst aus der appsettings.json.
// Fehlt beides, brechen wir sofort mit einer klaren Meldung ab – das ist besser als ein
// rätselhafter Fehler später.
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DATABASE_URL ist nicht gesetzt.");

// Die Datenbank-Verbindung anmelden und sagen: wir benutzen PostgreSQL mit dieser Adresse.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Das Repository anmelden: "wer ein IPersonRepository braucht, bekommt ein PersonRepository".
// "Scoped" heißt: pro Anfrage wird ein eigenes erzeugt.
builder.Services.AddScoped<IPersonRepository, PersonRepository>();

builder.Services.AddControllers();          // schaltet die Controller ein (z.B. unseren PersonsController).
builder.Services.AddEndpointsApiExplorer(); // sammelt Infos über die Anfragen – wird für die Doku-Seite gebraucht.
// Swagger = eine Web-Seite, die die API automatisch beschreibt und zum Ausprobieren einlädt
// (erreichbar unter /swagger).
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "PersonenVerwaltung API", Version = "v1" });
});

// CORS regelt, von welchen anderen Web-Adressen aus die API aufgerufen werden darf.
// Hier ist alles erlaubt – praktisch für die Demo. Für den echten Einsatz sollte man das enger fassen.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// --- Server bauen ---
var app = builder.Build();

// --- Ablauf festlegen (jede Anfrage läuft diese Schritte der Reihe nach durch) ---

app.UseSwagger();      // stellt die Beschreibung der API bereit.
app.UseSwaggerUI();    // stellt die anklickbare Doku-Seite bereit.

app.UseCors();         // wendet die oben festgelegte Zugriffsregel an.
app.MapControllers();  // leitet jede Anfrage an den passenden Controller weiter.

app.Run();             // startet den Server – ab hier nimmt er Anfragen entgegen.
