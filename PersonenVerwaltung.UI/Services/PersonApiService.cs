using System.Net.Http.Json;

namespace PersonenVerwaltung.UI.Services;

// --- Data Transfer Objects für die API-Antworten ---
// Die folgenden records spiegeln die JSON-Projektionen des API-Controllers wider.
// Die Feldnamen müssen mit den dort erzeugten anonymen Objekten übereinstimmen;
// Änderungen an der API sind hier nachzuziehen.

/// <summary>Ein Eintrag der Personenübersicht (Startseite).</summary>
public record PersonListItem(int Id, string Name, string Vorname, DateOnly Geburtsdatum);

/// <summary>Vollständige Detaildaten einer Person inklusive Anschriften und Telefonverbindungen.</summary>
public record PersonDetail(
    int Id,
    string Name,
    string Vorname,
    DateOnly Geburtsdatum,
    string? NameUppercase,
    List<AnschriftItem> Anschriften,
    List<TelefonItem> Telefonverbindungen);

/// <summary>Eine Anschrift innerhalb der Detaildaten.</summary>
public record AnschriftItem(int Id, string Postleitzahl, string Ort, string Strasse, string Hausnummer);

/// <summary>Eine Telefonverbindung innerhalb der Detaildaten.</summary>
public record TelefonItem(int Id, string Nummer);

/// <summary>
/// Typisierter HTTP-Client-Dienst, über den die Blazor-UI mit der REST-API kommuniziert.
/// Kapselt sämtliche API-Aufrufe an einer Stelle und stellt die Schichtentrennung sicher:
/// Die UI greift nie direkt auf die Datenbank zu (UI → HTTP → API → Datenschicht).
/// </summary>
public class PersonApiService
{
    private readonly HttpClient _http;

    /// <summary>
    /// Initialisiert den Dienst mit dem per Dependency Injection bereitgestellten
    /// <see cref="HttpClient"/>, dessen Basis-Adresse zentral in <c>Program.cs</c> der UI
    /// konfiguriert ist.
    /// </summary>
    /// <param name="http">Der typisierte HTTP-Client.</param>
    public PersonApiService(HttpClient http)
    {
        _http = http;
    }

    /// <summary>
    /// Lädt die Personenliste, optional gefiltert nach einem Suchbegriff.
    /// </summary>
    /// <param name="name">Optionaler Suchbegriff für Name oder Vorname.</param>
    /// <returns>Die Personenliste; bei leerer Antwort eine leere Liste statt <c>null</c>.</returns>
    public async Task<List<PersonListItem>> GetPersonenAsync(string? name = null)
    {
        // Suchbegriff URL-sicher kodieren, um fehlerhafte Anfragen zu vermeiden.
        var url = string.IsNullOrWhiteSpace(name)
            ? "api/persons"
            : $"api/persons?name={Uri.EscapeDataString(name)}";

        return await _http.GetFromJsonAsync<List<PersonListItem>>(url) ?? new();
    }

    /// <summary>
    /// Lädt die Detaildaten einer einzelnen Person.
    /// </summary>
    /// <param name="id">Primärschlüssel der Person.</param>
    /// <returns>Die Detaildaten oder <c>null</c>, wenn die API mit HTTP 404 antwortet.</returns>
    public async Task<PersonDetail?> GetPersonDetailAsync(int id)
    {
        return await _http.GetFromJsonAsync<PersonDetail>($"api/persons/{id}");
    }

    /// <summary>
    /// Speichert geänderten Name und Vorname einer Person per PUT.
    /// </summary>
    /// <param name="id">Primärschlüssel der Person.</param>
    /// <param name="name">Neuer Nachname.</param>
    /// <param name="vorname">Neuer Vorname.</param>
    /// <returns><c>true</c> bei erfolgreicher Antwort (HTTP 2xx), andernfalls <c>false</c>.</returns>
    public async Task<bool> UpdatePersonAsync(int id, string name, string vorname)
    {
        var response = await _http.PutAsJsonAsync($"api/persons/{id}", new { Name = name, Vorname = vorname });
        return response.IsSuccessStatusCode;
    }
}
