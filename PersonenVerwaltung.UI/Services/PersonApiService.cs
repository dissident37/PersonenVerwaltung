using System.Net.Http.Json;     // Erweiterungen wie GetFromJsonAsync/PutAsJsonAsync: JSON <-> C#-Objekte automatisch.

namespace PersonenVerwaltung.UI.Services;

// --- Datenobjekte (DTOs) für die API-Antworten ---
// "record" = kurze, unveränderliche Daten-Klasse. Ideal, um Daten nur zu transportieren.
// Diese records müssen zur JSON-Form passen, die die API zurückgibt (gleiche Feldnamen!).
// WICHTIG (Spickzettel): Diese Formen sind eine "Spiegelung" der anonymen Objekte im API-Controller.
//                        Ändert sich dort ein Feld, muss es hier mitgeändert werden.

// Ein Eintrag der Listen-Übersicht (Startseite).
public record PersonListItem(int Id, string Name, string Vorname, DateOnly Geburtsdatum);

// Die vollständigen Detaildaten einer Person (Detailseite), inkl. verschachtelter Listen.
public record PersonDetail(
    int Id,
    string Name,
    string Vorname,
    DateOnly Geburtsdatum,
    string? NameUppercase,                 // optional (nullable) – kann fehlen.
    List<AnschriftItem> Anschriften,       // Liste der Adressen.
    List<TelefonItem> Telefonverbindungen);// Liste der Telefonnummern.

public record AnschriftItem(int Id, string Postleitzahl, string Ort, string Strasse, string Hausnummer);
public record TelefonItem(int Id, string Nummer);

// WAS IST DAS: Der HTTP-Client-Dienst, über den die Blazor-UI mit der REST-API spricht.
// WARUM BRAUCHEN WIR DAS: Die UI greift NIE direkt auf die Datenbank zu – sie redet nur über HTTP mit der API.
//                         Dieser Service bündelt alle API-Aufrufe an einer Stelle.
// IM VORSTELLUNGSGESPRÄCH SAGEN:
//   "Das ist ein typisierter HttpClient. Den HttpClient bekomme ich per Dependency Injection in den Konstruktor;
//    seine Basis-Adresse (API_URL) ist zentral in Program.cs der UI konfiguriert. Die JSON-Umwandlung machen
//    die Json-Helfer (GetFromJsonAsync, PutAsJsonAsync) – ich muss nicht von Hand parsen. So bleibt die
//    Trennung sauber: UI -> HTTP -> API -> Datenbank."
public class PersonApiService
{
    private readonly HttpClient _http;     // das Werkzeug zum Senden von HTTP-Anfragen.

    // Konstruktor: HttpClient kommt per Dependency Injection rein (in der UI registriert, inkl. Basis-Adresse).
    public PersonApiService(HttpClient http)
    {
        _http = http;
    }

    // WAS: Holt die Personenliste, optional gefiltert nach einem Suchtext.
    // WARUM: Versorgt die Tabelle auf der Startseite mit Daten.
    public async Task<List<PersonListItem>> GetPersonenAsync(string? name = null)
    {
        // URL zusammenbauen: ohne Suchtext die einfache Liste, sonst mit Query-Parameter ?name=...
        // Uri.EscapeDataString = macht den Text URL-sicher (z.B. Leerzeichen -> %20), schützt vor kaputten URLs.
        var url = string.IsNullOrWhiteSpace(name)
            ? "api/persons"
            : $"api/persons?name={Uri.EscapeDataString(name)}";

        // GetFromJsonAsync = GET-Anfrage + JSON automatisch in List<PersonListItem> umwandeln.
        // "?? new()" = falls die Antwort null ist, lieber eine leere Liste zurückgeben (kein null nach außen).
        return await _http.GetFromJsonAsync<List<PersonListItem>>(url) ?? new();
    }

    // WAS: Holt die Detaildaten EINER Person per Id.
    // WARUM: Versorgt die Detailseite. Rückgabe ist nullable -> bei 404 kommt null zurück.
    public async Task<PersonDetail?> GetPersonDetailAsync(int id)
    {
        return await _http.GetFromJsonAsync<PersonDetail>($"api/persons/{id}");
    }

    // WAS: Schickt geänderten Name + Vorname per PUT an die API.
    // WARUM: Setzt das Speichern der Inline-Bearbeitung um.
    // Rückgabe bool = true bei Erfolg, damit die UI weiß, ob sie die Liste neu laden soll.
    public async Task<bool> UpdatePersonAsync(int id, string name, string vorname)
    {
        // PutAsJsonAsync = PUT-Anfrage; das anonyme Objekt { Name, Vorname } wird automatisch zu JSON.
        var response = await _http.PutAsJsonAsync($"api/persons/{id}", new { Name = name, Vorname = vorname });
        // IsSuccessStatusCode = true bei HTTP 2xx (z.B. 204 No Content), false bei Fehlern (z.B. 400/404).
        return response.IsSuccessStatusCode;
    }
}
