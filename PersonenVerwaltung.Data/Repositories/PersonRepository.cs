using Microsoft.EntityFrameworkCore;
using PersonenVerwaltung.Data.Models;

namespace PersonenVerwaltung.Data.Repositories;

/// <summary>
/// Entity-Framework-Core-Implementierung von <see cref="IPersonRepository"/>.
/// Bündelt sämtlichen Datenbankzugriff auf Personen an einer Stelle und übersetzt die
/// LINQ-Abfragen in SQL.
/// </summary>
public class PersonRepository : IPersonRepository
{
    private readonly AppDbContext _db;

    /// <summary>
    /// Initialisiert das Repository mit dem per Dependency Injection bereitgestellten
    /// <see cref="AppDbContext"/>.
    /// </summary>
    /// <param name="db">Der Datenbankkontext (Lebensdauer: scoped, je HTTP-Anfrage).</param>
    public PersonRepository(AppDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Person>> GetAllAsync(string? nameFilter = null)
    {
        // Schrittweise aufgebaute Abfrage; die Ausführung erfolgt verzögert erst durch
        // ToListAsync (Deferred Execution), sodass der Filter Teil des SQL-WHERE wird.
        var query = _db.Personen.AsQueryable();

        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            var filter = nameFilter.ToLower();
            // Case-insensitive Teilstringsuche über Name oder Vorname (übersetzt zu LIKE).
            query = query.Where(p =>
                p.Name.ToLower().Contains(filter) ||
                p.Vorname.ToLower().Contains(filter));
        }

        return await query.OrderBy(p => p.Name).ThenBy(p => p.Vorname).ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Person?> GetByIdAsync(int id)
    {
        return await _db.Personen
            // Eager Loading: Anschriften und Telefonverbindungen werden in derselben
            // Abfrage geladen, um das N+1-Problem zu vermeiden.
            .Include(p => p.Anschriften)
            .Include(p => p.Telefonverbindungen)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <inheritdoc />
    public async Task UpdateNameAsync(int id, string name, string vorname)
    {
        var person = await _db.Personen.FindAsync(id);
        if (person == null) return;

        person.Name = name;
        person.Vorname = vorname;
        // Denormalisiertes Feld konsistent zum Namen halten.
        person.NameUppercase = name.ToUpper();

        // Das Change Tracking von EF Core ermittelt das erforderliche UPDATE; erst hier
        // wird die Änderung persistiert.
        await _db.SaveChangesAsync();
    }
}
