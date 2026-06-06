using Microsoft.AspNetCore.Mvc;          // das Web-API-Framework (Controller, IActionResult, Attribute ...).
using PersonenVerwaltung.Data.Models;
using PersonenVerwaltung.Data.Repositories;

namespace PersonenVerwaltung.API.Controllers;

// WAS IST DAS: Der API-Controller – die Klasse, die die HTTP-Anfragen (GET/PUT) für Personen entgegennimmt.
// WARUM BRAUCHEN WIR DAS: Er ist die "Tür" des Web-Services. Die Blazor-UI spricht NUR über HTTP mit ihm;
//                         er ruft das Repository auf und gibt die Daten als JSON zurück.
// IM VORSTELLUNGSGESPRÄCH SAGEN:
//   "Das ist ein REST-Controller. '[ApiController]' aktiviert API-Komfort wie automatische Modell-Validierung.
//    '[Route]' legt die Basis-URL fest. Jede Methode ist ein Endpunkt. Wichtig: Ich gebe nicht die EF-Entitäten
//    direkt zurück, sondern eine projizierte (zugeschnittene) Form – so leake ich keine internen Felder und
//    vermeide Endlosschleifen beim JSON-Serialisieren (Person -> Anschrift -> Person -> ...)."

// Attribute stehen in eckigen Klammern und geben dem Framework Anweisungen:
[ApiController]               // markiert die Klasse als Web-API-Controller (z.B. automatische 400 bei ungültigem Body).
[Route("api/persons")]       // Basis-Adresse: alle Endpunkte hier beginnen mit /api/persons.
public class PersonsController : ControllerBase   // ControllerBase = Basisklasse für APIs (ohne View-Kram, anders als MVC-"Controller").
{
    private readonly IPersonRepository _repo;     // wir hängen am Interface, nicht an der konkreten Klasse (lose Kopplung).

    // Konstruktor: das Repository kommt per Dependency Injection rein (in Program.cs registriert).
    public PersonsController(IPersonRepository repo)
    {
        _repo = repo;
    }

    // GET /api/persons?name=...   -> Liste aller Personen, optional nach Name/Vorname gefiltert.
    // WAS: Liefert die Übersichtsliste für die Startseite.
    // WARUM: Die UI ruft diesen Endpunkt beim Laden und beim Suchen auf.
    [HttpGet]                                       // dieser Endpunkt reagiert auf HTTP GET.
    public async Task<IActionResult> GetAll([FromQuery] string? name)   // [FromQuery] = "name" kommt aus der URL (?name=...).
    {
        var persons = await _repo.GetAllAsync(name);
        // Select(...) = "projizieren": wir bauen aus jeder Person ein neues, schlankes Objekt nur mit den
        // Feldern, die die Liste braucht. "new { ... }" = anonymer Typ (Objekt ohne eigenen Klassennamen).
        var result = persons.Select(p => new
        {
            p.Id,
            p.Name,
            p.Vorname,
            p.Geburtsdatum
        });
        return Ok(result);     // Ok(...) = HTTP-Status 200 + die Daten als JSON.
    }

    // GET /api/persons/{id}   -> eine einzelne Person mit allen Details.
    // WAS: Liefert Person + Anschriften + Telefonnummern für die Detailseite.
    // WARUM: Die Detailansicht braucht das komplette Bild zu einer Person.
    [HttpGet("{id:int}")]      // {id:int} = Platzhalter in der URL, der eine Ganzzahl sein muss (Route Constraint).
    public async Task<IActionResult> GetById(int id)
    {
        var person = await _repo.GetByIdAsync(id);
        if (person == null) return NotFound();      // NotFound() = HTTP 404, wenn es die Person nicht gibt.

        // Auch hier projizieren wir auf eine zugeschnittene Form – inkl. der verschachtelten Listen.
        var result = new
        {
            person.Id,
            person.Name,
            person.Vorname,
            person.Geburtsdatum,
            person.NameUppercase,
            // verschachtelte Projektion: jede Anschrift wird ebenfalls auf die nötigen Felder reduziert.
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
        return Ok(result);     // HTTP 200 + JSON.
    }

    // PUT /api/persons/{id}   -> ändert Name + Vorname einer Person.
    // WAS: Nimmt die geänderten Werte aus dem Request-Body entgegen und speichert sie.
    // WARUM: Über diesen Endpunkt funktioniert die Inline-Bearbeitung in der UI.
    // (PUT = "ersetze/aktualisiere eine vorhandene Ressource" im REST-Sinn.)
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateName(int id, [FromBody] UpdatePersonRequest request)  // [FromBody] = Daten kommen als JSON im Anfrage-Körper.
    {
        // Eingabe-Validierung: leere Werte ablehnen, bevor wir die DB anfassen.
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Vorname))
            return BadRequest("Name und Vorname duerfen nicht leer sein.");   // BadRequest() = HTTP 400.

        var person = await _repo.GetByIdAsync(id);
        if (person == null) return NotFound();      // existiert die Person nicht -> 404.

        await _repo.UpdateNameAsync(id, request.Name, request.Vorname);
        return NoContent();    // NoContent() = HTTP 204: "erfolgreich, aber ich sende keinen Inhalt zurück".
    }
}

// record = kompakte, unveränderliche (immutable) Daten-Klasse. Perfekt als reiner Daten-Transport (DTO).
// "DTO" = Data Transfer Object = Objekt, das nur Daten zwischen UI und API hin- und herträgt.
// Diese Form erwartet der PUT-Endpunkt im JSON-Body: { "Name": "...", "Vorname": "..." }.
public record UpdatePersonRequest(string Name, string Vorname);
