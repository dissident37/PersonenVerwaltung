using Microsoft.AspNetCore.Mvc;
using PersonenVerwaltung.Data.Models;
using PersonenVerwaltung.Data.Repositories;

namespace PersonenVerwaltung.API.Controllers;

/// <summary>
/// REST-Controller für Personen und HTTP-Einstiegspunkt der Anwendung. Bildet die
/// API-Schicht der Drei-Schichten-Architektur und delegiert sämtliche Datenzugriffe an
/// <see cref="IPersonRepository"/>. Die Blazor-UI kommuniziert ausschließlich über diesen
/// Controller (HTTP/JSON).
/// </summary>
/// <remarks>
/// Die Endpunkte liefern bewusst zugeschnittene Projektionen statt der EF-Entitäten. Das
/// vermeidet das Offenlegen interner Felder sowie Zyklen bei der JSON-Serialisierung
/// (Person → Anschrift → Person → …).
/// </remarks>
[ApiController]
[Route("api/persons")]
public class PersonsController : ControllerBase
{
    // Abhängigkeit auf die Abstraktion statt auf die konkrete Implementierung (lose Kopplung).
    private readonly IPersonRepository _repo;

    /// <summary>
    /// Initialisiert den Controller mit dem per Dependency Injection bereitgestellten
    /// Repository.
    /// </summary>
    /// <param name="repo">Datenzugriffsabstraktion für Personen.</param>
    public PersonsController(IPersonRepository repo)
    {
        _repo = repo;
    }

    /// <summary>
    /// Liefert die Personenliste, optional gefiltert nach Name oder Vorname.
    /// </summary>
    /// <param name="name">Optionaler Suchbegriff aus der Query (<c>?name=...</c>).</param>
    /// <returns>HTTP 200 mit der projizierten Personenliste.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? name)
    {
        var persons = await _repo.GetAllAsync(name);
        // Projektion auf die für die Übersicht benötigten Felder.
        var result = persons.Select(p => new
        {
            p.Id,
            p.Name,
            p.Vorname,
            p.Geburtsdatum
        });
        return Ok(result);
    }

    /// <summary>
    /// Liefert eine einzelne Person mit allen Detaildaten.
    /// </summary>
    /// <param name="id">Primärschlüssel der Person.</param>
    /// <returns>
    /// HTTP 200 mit der Person inklusive Anschriften und Telefonverbindungen, andernfalls HTTP 404.
    /// </returns>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var person = await _repo.GetByIdAsync(id);
        if (person == null) return NotFound();

        // Projektion inklusive der verschachtelten Beziehungen.
        var result = new
        {
            person.Id,
            person.Name,
            person.Vorname,
            person.Geburtsdatum,
            person.NameUppercase,
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
        return Ok(result);
    }

    /// <summary>
    /// Aktualisiert Name und Vorname einer Person.
    /// </summary>
    /// <param name="id">Primärschlüssel der zu ändernden Person.</param>
    /// <param name="request">Die neuen Werte für Name und Vorname.</param>
    /// <returns>
    /// HTTP 204 bei Erfolg, HTTP 400 bei leeren Werten, HTTP 404 bei unbekannter Id.
    /// </returns>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateName(int id, [FromBody] UpdatePersonRequest request)
    {
        // Eingabevalidierung vor dem Datenbankzugriff.
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Vorname))
            return BadRequest("Name und Vorname duerfen nicht leer sein.");

        var person = await _repo.GetByIdAsync(id);
        if (person == null) return NotFound();

        await _repo.UpdateNameAsync(id, request.Name, request.Vorname);
        return NoContent();
    }
}

/// <summary>
/// Data Transfer Object für den PUT-Endpunkt. Trägt die zu ändernden Felder
/// (Name, Vorname) aus dem JSON-Anfragekörper.
/// </summary>
public record UpdatePersonRequest(string Name, string Vorname);
