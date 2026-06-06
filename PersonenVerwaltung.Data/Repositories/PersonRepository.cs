using Microsoft.EntityFrameworkCore;    // für LINQ-Erweiterungen wie ToListAsync, Include, FirstOrDefaultAsync.
using PersonenVerwaltung.Data.Models;

namespace PersonenVerwaltung.Data.Repositories;

// WAS IST DAS: Die konkrete Umsetzung des IPersonRepository-Vertrags mit Entity Framework Core.
// WARUM BRAUCHEN WIR DAS: Hier passiert der echte Datenbank-Zugriff (Lesen, Suchen, Speichern).
//                         Alle DB-Logik ist an dieser einen Stelle gebündelt.
// IM VORSTELLUNGSGESPRÄCH SAGEN:
//   "Diese Klasse implementiert das Interface (': IPersonRepository'). Den AppDbContext bekomme ich per
//    Dependency Injection in den Konstruktor gereicht – ich erzeuge ihn nicht selbst. Abfragen schreibe ich
//    mit LINQ; EF Core macht daraus SQL. Mit 'async/await' bleibt der Server bei DB-Wartezeiten reaktionsfähig."
public class PersonRepository : IPersonRepository
{
    // "readonly" = Feld kann nur im Konstruktor gesetzt werden, danach nicht mehr -> schützt vor versehentlichem Überschreiben.
    // "_db" mit Unterstrich = Konvention für private Felder einer Klasse.
    private readonly AppDbContext _db;

    // Konstruktor: bekommt den DbContext von außen (Dependency Injection). DI = das Framework
    // baut benötigte Objekte selbst zusammen und reicht sie rein -> wir müssen sie nicht selbst erzeugen.
    public PersonRepository(AppDbContext db)
    {
        _db = db;
    }

    // WAS MACHT DIE METHODE: Liefert alle Personen; ist ein Suchtext da, filtert sie nach Name ODER Vorname.
    // WARUM: Die Übersichtsliste in der UI braucht alle Personen, mit optionaler Suche.
    public async Task<IEnumerable<Person>> GetAllAsync(string? nameFilter = null)
    {
        // AsQueryable() = startet eine Abfrage, die noch NICHT ausgeführt ist. Wir können sie schrittweise
        // zusammenbauen (Filter, Sortierung). Ausgeführt wird erst am Ende mit ToListAsync() -> "deferred execution".
        var query = _db.Personen.AsQueryable();

        // IsNullOrWhiteSpace = true, wenn der Text null, leer oder nur Leerzeichen ist -> dann nicht filtern.
        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            var filter = nameFilter.ToLower();        // alles klein schreiben -> Suche unabhängig von Groß/Klein.
            // Where(...) = SQL-WHERE. "p =>" ist ein Lambda: "für jede Person p gilt diese Bedingung".
            // Contains = Teilstring-Suche (LIKE '%filter%'). "||" = ODER.
            query = query.Where(p =>
                p.Name.ToLower().Contains(filter) ||
                p.Vorname.ToLower().Contains(filter));
        }

        // OrderBy = sortieren nach Name, ThenBy = bei gleichem Namen zusätzlich nach Vorname.
        // ToListAsync() = JETZT wird die Abfrage an die DB geschickt und das Ergebnis als Liste geladen.
        // "await" = warte (nicht-blockierend) auf das Ergebnis der DB.
        return await query.OrderBy(p => p.Name).ThenBy(p => p.Vorname).ToListAsync();
    }

    // WAS MACHT DIE METHODE: Holt EINE Person samt ihrer Anschriften und Telefonnummern.
    // WARUM: Die Detailseite braucht alle zugehörigen Daten in einem Rutsch.
    public async Task<Person?> GetByIdAsync(int id)
    {
        return await _db.Personen
            // Include = "lade auch die verknüpften Daten mit" (sonst wären Anschriften/Telefon leer).
            // Das nennt man "Eager Loading" -> alles in einer Abfrage statt vieler Einzelabfragen.
            .Include(p => p.Anschriften)
            .Include(p => p.Telefonverbindungen)
            // FirstOrDefaultAsync = erste passende Person ODER null (default), wenn keine die Id hat.
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    // WAS MACHT DIE METHODE: Aktualisiert Name + Vorname einer Person und hält das Feld NameUppercase aktuell.
    // WARUM: Die UI darf nur Name/Vorname ändern. NameUppercase ist eine "redundante" Kopie in Großbuchstaben,
    //        die laut Vorgabe immer mitgepflegt werden muss.
    public async Task UpdateNameAsync(int id, string name, string vorname)
    {
        // FindAsync = sucht effizient per Primärschlüssel (Id). Schaut zuerst im Speicher, dann in der DB.
        var person = await _db.Personen.FindAsync(id);
        if (person == null) return;     // Person existiert nicht -> nichts tun, sauber rausgehen.

        // Wir ändern die Eigenschaften am geladenen Objekt. EF Core merkt sich diese Änderungen
        // automatisch ("Change Tracking") und weiß so, welches UPDATE es schicken muss.
        person.Name = name;
        person.Vorname = vorname;
        person.NameUppercase = name.ToUpper();   // denormalisierte Spalte synchron halten (Großschreibung des Namens).

        // SaveChangesAsync = jetzt erst werden die gemerkten Änderungen als SQL-UPDATE in die DB geschrieben.
        await _db.SaveChangesAsync();
    }
}
