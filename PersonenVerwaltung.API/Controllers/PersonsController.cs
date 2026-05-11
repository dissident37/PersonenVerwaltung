using Microsoft.AspNetCore.Mvc;
using PersonenVerwaltung.Data.Models;
using PersonenVerwaltung.Data.Repositories;

namespace PersonenVerwaltung.API.Controllers;

[ApiController]
[Route("api/persons")]
public class PersonsController : ControllerBase
{
    private readonly IPersonRepository _repo;

    public PersonsController(IPersonRepository repo)
    {
        _repo = repo;
    }

    // GET /api/persons?name=...
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? name)
    {
        var persons = await _repo.GetAllAsync(name);
        var result = persons.Select(p => new
        {
            p.Id,
            p.Name,
            p.Vorname,
            p.Geburtsdatum
        });
        return Ok(result);
    }

    // GET /api/persons/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var person = await _repo.GetByIdAsync(id);
        if (person == null) return NotFound();

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

    // PUT /api/persons/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateName(int id, [FromBody] UpdatePersonRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Vorname))
            return BadRequest("Name und Vorname duerfen nicht leer sein.");

        var person = await _repo.GetByIdAsync(id);
        if (person == null) return NotFound();

        await _repo.UpdateNameAsync(id, request.Name, request.Vorname);
        return NoContent();
    }
}

public record UpdatePersonRequest(string Name, string Vorname);
