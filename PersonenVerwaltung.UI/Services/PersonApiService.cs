using System.Net.Http.Json;

namespace PersonenVerwaltung.UI.Services;

// Datenobjekte fuer die API-Antworten
public record PersonListItem(int Id, string Name, string Vorname, DateOnly Geburtsdatum);

public record PersonDetail(
    int Id,
    string Name,
    string Vorname,
    DateOnly Geburtsdatum,
    string? NameUppercase,
    List<AnschriftItem> Anschriften,
    List<TelefonItem> Telefonverbindungen);

public record AnschriftItem(int Id, string Postleitzahl, string Ort, string Strasse, string Hausnummer);
public record TelefonItem(int Id, string Nummer);

public class PersonApiService
{
    private readonly HttpClient _http;

    public PersonApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<PersonListItem>> GetPersonenAsync(string? name = null)
    {
        var url = string.IsNullOrWhiteSpace(name)
            ? "api/persons"
            : $"api/persons?name={Uri.EscapeDataString(name)}";

        return await _http.GetFromJsonAsync<List<PersonListItem>>(url) ?? new();
    }

    public async Task<PersonDetail?> GetPersonDetailAsync(int id)
    {
        return await _http.GetFromJsonAsync<PersonDetail>($"api/persons/{id}");
    }

    public async Task<bool> UpdatePersonAsync(int id, string name, string vorname)
    {
        var response = await _http.PutAsJsonAsync($"api/persons/{id}", new { Name = name, Vorname = vorname });
        return response.IsSuccessStatusCode;
    }
}
