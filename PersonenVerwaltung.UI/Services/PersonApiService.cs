using System.Net.Http.Json;

namespace PersonenVerwaltung.UI.Services;

// --- Kleine Daten-Klassen für die Antworten der API ---
// Diese Klassen beschreiben, wie die Daten aussehen, die von der API zurückkommen.
// Wichtig: Sie müssen genau zu dem passen, was der Controller in der API zurückgibt
// (gleiche Feldnamen). Ändert sich dort etwas, muss es hier ebenfalls geändert werden.

// Ein Eintrag in der Personen-Übersicht (auf der Startseite).
public record PersonListItem(int Id, string Name, string Vorname, DateOnly Geburtsdatum);

// Die vollständigen Daten einer Person für die Detailseite, inklusive Adressen und Telefonnummern.
public record PersonDetail(
    int Id,
    string Name,
    string Vorname,
    DateOnly Geburtsdatum,
    string? NameUppercase,                 // darf leer sein (das Fragezeichen).
    List<AnschriftItem> Anschriften,       // Liste der Adressen.
    List<TelefonItem> Telefonverbindungen);// Liste der Telefonnummern.

// Eine einzelne Adresse innerhalb der Detaildaten.
public record AnschriftItem(int Id, string Postleitzahl, string Ort, string Strasse, string Hausnummer);

// Eine einzelne Telefonnummer innerhalb der Detaildaten.
public record TelefonItem(int Id, string Nummer);

// Dieser Helfer verschickt die Anfragen von der Oberfläche an die API und nimmt die Antworten entgegen.
// Die Oberfläche greift NIE direkt auf die Datenbank zu – sie redet nur über diesen Helfer mit der API.
// So sind alle API-Aufrufe an einer Stelle gebündelt.
public class PersonApiService
{
    // Das Werkzeug zum Verschicken der Anfragen.
    private readonly HttpClient _http;

    // Beim Erzeugen wird dieses Werkzeug von außen hereingereicht – samt fest eingestellter API-Adresse.
    public PersonApiService(HttpClient http)
    {
        _http = http;
    }

    // Holt die Personenliste. Mit einem Suchtext wird nach Name oder Vorname gefiltert.
    public async Task<List<PersonListItem>> GetPersonenAsync(string? name = null)
    {
        // Adresse zusammenbauen: ohne Suchtext die einfache Liste, sonst mit Suchteil ?name=...
        // EscapeDataString macht den Suchtext sicher für die Adresse (z.B. wird ein Leerzeichen umgewandelt).
        var url = string.IsNullOrWhiteSpace(name)
            ? "api/persons"
            : $"api/persons?name={Uri.EscapeDataString(name)}";

        // Anfrage abschicken und die Antwort gleich in eine Liste umwandeln.
        // Kommt nichts zurück, geben wir lieber eine leere Liste zurück als "nichts" (null).
        return await _http.GetFromJsonAsync<List<PersonListItem>>(url) ?? new();
    }

    // Holt die Detaildaten EINER Person anhand ihrer Nummer.
    // Gibt es die Person nicht (Antwort 404), kommt "nichts" (null) zurück.
    public async Task<PersonDetail?> GetPersonDetailAsync(int id)
    {
        return await _http.GetFromJsonAsync<PersonDetail>($"api/persons/{id}");
    }

    // Schickt geänderten Name und Vorname an die API.
    // Gibt "true" zurück, wenn es geklappt hat – daran erkennt die Oberfläche, ob sie neu laden soll.
    public async Task<bool> UpdatePersonAsync(int id, string name, string vorname)
    {
        // Die neuen Werte als Anfrage abschicken.
        var response = await _http.PutAsJsonAsync($"api/persons/{id}", new { Name = name, Vorname = vorname });
        // "true", wenn die Antwort einen Erfolg meldet; "false" bei einem Fehler.
        return response.IsSuccessStatusCode;
    }
}
