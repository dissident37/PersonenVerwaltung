using Microsoft.AspNetCore.Mvc;
using PersonenVerwaltung.Data.Models;
using PersonenVerwaltung.Data.Repositories;

namespace PersonenVerwaltung.API.Controllers;

// Das ist der Controller (die Anlaufstelle) für alle Web-Anfragen rund um Personen.
// Hier kommen die Anfragen aus dem Internet an (z.B. "gib mir alle Personen").
// Der Controller holt die Daten über das Repository und schickt sie als Antwort zurück.
// Die Benutzeroberfläche (UI) spricht NUR über solche Anfragen mit dem Programm,
// nie direkt mit der Datenbank.
//
// Hinweis: Der Controller gibt nicht die kompletten Person-Objekte zurück, sondern baut sich
// jeweils eine abgespeckte Version nur mit den nötigen Feldern. So geben wir nichts Unnötiges
// nach außen und vermeiden Endlosschleifen beim Umwandeln in das Antwortformat.

[ApiController]               // sagt: das ist ein Controller für eine Web-Schnittstelle.
[Route("api/persons")]       // Grundadresse: alle Anfragen hier beginnen mit /api/persons.
public class PersonsController : ControllerBase
{
    // Über dieses Repository (den Daten-Helfer) kommt der Controller an die Personen-Daten.
    // Er kennt nur den Vertrag (das Interface), nicht die konkrete Umsetzung.
    private readonly IPersonRepository _repo;

    // Beim Erzeugen wird das Repository von außen hereingereicht.
    public PersonsController(IPersonRepository repo)
    {
        _repo = repo;
    }

    // Anfrage: GET /api/persons?name=...
    // Gibt die Liste aller Personen zurück. Mit "name" kann man nach Name oder Vorname suchen.
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? name)
    {
        var persons = await _repo.GetAllAsync(name);
        // Aus jeder Person nur die Felder herausnehmen, die die Übersichtsliste braucht.
        var result = persons.Select(p => new
        {
            p.Id,
            p.Name,
            p.Vorname,
            p.Geburtsdatum
        });
        return Ok(result);     // Ok = "alles in Ordnung" (Status 200), dazu die Daten.
    }

    // Anfrage: GET /api/persons/{id}
    // Gibt eine einzelne Person mit allen Details zurück (inklusive Adressen und Telefonnummern).
    [HttpGet("{id:int}")]      // {id:int} = an dieser Stelle der Adresse muss eine Zahl stehen.
    public async Task<IActionResult> GetById(int id)
    {
        var person = await _repo.GetByIdAsync(id);
        // Gibt es die Person nicht, antworten wir mit "nicht gefunden" (Status 404).
        if (person == null) return NotFound();

        // Auch hier wieder eine abgespeckte Version bauen – diesmal mit den Adressen und Nummern.
        var result = new
        {
            person.Id,
            person.Name,
            person.Vorname,
            person.Geburtsdatum,
            person.NameUppercase,
            // Auch jede Adresse auf die nötigen Felder reduzieren.
            Anschriften = person.Anschriften.Select(a => new
            {
                a.Id,
                a.Postleitzahl,
                a.Ort,
                a.Strasse,
                a.Hausnummer
            }),
            Telefonverbindungen = person.Telefonverbindungen.Select(t => new
            {
                t.Id,
                t.Nummer
            })
        };
        return Ok(result);     // Status 200 + die Daten.
    }

    // Anfrage: PUT /api/persons/{id}
    // Ändert Name und Vorname einer Person. Die neuen Werte stehen im Inhalt der Anfrage.
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateName(int id, [FromBody] UpdatePersonRequest request)
    {
        // Erst prüfen: Name und Vorname dürfen nicht leer sein. Sonst sofort ablehnen (Status 400).
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Vorname))
            return BadRequest("Name und Vorname duerfen nicht leer sein.");

        var person = await _repo.GetByIdAsync(id);
        // Gibt es die Person nicht, antworten wir mit "nicht gefunden" (Status 404).
        if (person == null) return NotFound();

        await _repo.UpdateNameAsync(id, request.Name, request.Vorname);
        return NoContent();    // Status 204: "hat geklappt, ich schicke aber nichts zurück".
    }
}

// Kleine Hilfsklasse nur zum Übertragen von Daten.
// Genau diese Form (Name + Vorname) erwartet die PUT-Anfrage in ihrem Inhalt.
public record UpdatePersonRequest(string Name, string Vorname);
