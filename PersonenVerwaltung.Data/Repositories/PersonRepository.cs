using Microsoft.EntityFrameworkCore;
using PersonenVerwaltung.Data.Models;

namespace PersonenVerwaltung.Data.Repositories;

// Diese Klasse erledigt den eigentlichen Datenbank-Zugriff für Personen:
// Lesen, Suchen und Speichern. Sie setzt den Vertrag aus IPersonRepository in die Tat um
// (deshalb steht ": IPersonRepository" oben).
// Der ganze Umgang mit der Datenbank ist hier an einer Stelle gebündelt.
public class PersonRepository : IPersonRepository
{
    // Die Verbindung zur Datenbank. "readonly" heißt: einmal gesetzt, danach nicht mehr änderbar.
    private readonly AppDbContext _db;

    // Beim Erzeugen wird die Datenbank-Verbindung von außen hereingereicht.
    // Wir müssen sie nicht selbst aufbauen – das macht das Programm für uns.
    public PersonRepository(AppDbContext db)
    {
        _db = db;
    }

    // Holt alle Personen. Ist ein Suchtext da, werden nur Personen geliefert,
    // deren Name oder Vorname diesen Text enthält. Am Ende wird nach Name und Vorname sortiert.
    public async Task<IEnumerable<Person>> GetAllAsync(string? nameFilter = null)
    {
        // Hier wird die Abfrage erst Schritt für Schritt zusammengebaut.
        // Sie wird noch nicht ausgeführt – das passiert erst ganz unten.
        var query = _db.Personen.AsQueryable();

        // Nur filtern, wenn wirklich ein Suchtext da ist (nicht leer, nicht nur Leerzeichen).
        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            // Suchtext klein schreiben, damit Groß-/Kleinschreibung bei der Suche egal ist.
            var filter = nameFilter.ToLower();
            // Behalte nur Personen, deren Name ODER Vorname den Suchtext enthält.
            query = query.Where(p =>
                p.Name.ToLower().Contains(filter) ||
                p.Vorname.ToLower().Contains(filter));
        }

        // Jetzt erst wird die fertige Abfrage an die Datenbank geschickt und das Ergebnis geholt.
        // Sortiert wird nach Name, bei gleichem Namen zusätzlich nach Vorname.
        return await query.OrderBy(p => p.Name).ThenBy(p => p.Vorname).ToListAsync();
    }

    // Holt EINE Person samt ihren Adressen und Telefonnummern.
    public async Task<Person?> GetByIdAsync(int id)
    {
        return await _db.Personen
            // Mit "Include" laden wir die verknüpften Daten gleich mit –
            // sonst blieben Adressen und Telefonnummern leer.
            .Include(p => p.Anschriften)
            .Include(p => p.Telefonverbindungen)
            // Nimm die erste Person mit dieser Nummer. Gibt es keine, kommt "nichts" (null) zurück.
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    // Ändert Name und Vorname einer Person und hält das Großbuchstaben-Feld dabei aktuell.
    public async Task UpdateNameAsync(int id, string name, string vorname)
    {
        // Person anhand ihrer Nummer suchen.
        var person = await _db.Personen.FindAsync(id);
        // Gibt es die Person nicht, brechen wir einfach ab und tun nichts.
        if (person == null) return;

        // Neue Werte setzen.
        person.Name = name;
        person.Vorname = vorname;
        // Die Großbuchstaben-Kopie des Namens passend nachziehen.
        person.NameUppercase = name.ToUpper();

        // Erst hier werden die Änderungen wirklich in die Datenbank geschrieben.
        await _db.SaveChangesAsync();
    }
}
