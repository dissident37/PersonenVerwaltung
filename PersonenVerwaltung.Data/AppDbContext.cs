using Microsoft.EntityFrameworkCore;
using PersonenVerwaltung.Data.Models;

namespace PersonenVerwaltung.Data;

// Das ist die zentrale Verbindung zur Datenbank.
// Über diese Klasse liest und schreibt das Programm alle Daten – wir müssen dafür kein SQL
// selbst schreiben, das übernimmt Entity Framework Core (ein Werkzeug, das C#-Objekte und
// Datenbank-Tabellen automatisch ineinander übersetzt).
// Alle anderen Teile, die mit Daten arbeiten, gehen über diese Klasse.
public class AppDbContext : DbContext
{
    // Beim Erzeugen werden die Einstellungen (z.B. welche Datenbank, welches Passwort) von außen
    // hereingereicht. "options" enthält diese Einstellungen; sie kommen aus der Program.cs.
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Jede dieser drei Zeilen steht für eine Tabelle in der Datenbank.
    // Über sie stellen wir später unsere Abfragen.
    public DbSet<Person> Personen { get; set; }                          // Tabelle "Person"
    public DbSet<Anschrift> Anschriften { get; set; }                    // Tabelle "Anschrift"
    public DbSet<Telefonverbindung> Telefonverbindungen { get; set; }    // Tabelle "Telefonverbindung"

    // Hier stellen wir genau ein, wie unsere Klassen zu den Tabellen passen.
    // Das ist nötig, weil die Tabellen schon fertig in der Datei database/init.sql angelegt sind –
    // wir müssen dem Programm nur sagen, welche Klasse zu welcher Tabelle gehört und wie die
    // Verbindungen zwischen den Tabellen aussehen.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Von sich aus würde das Programm Tabellennamen in der Mehrzahl erwarten (z.B. "People").
        // Unsere Tabellen heißen aber in der Einzahl. Deshalb sagen wir die Namen hier von Hand.
        modelBuilder.Entity<Person>().ToTable("Person");
        modelBuilder.Entity<Anschrift>().ToTable("Anschrift");
        modelBuilder.Entity<Telefonverbindung>().ToTable("Telefonverbindung");

        // Verbindung zwischen Anschrift und Person festlegen:
        // - Eine Anschrift gehört zu EINER Person.
        // - Eine Person kann VIELE Anschriften haben.
        // - Verbunden werden sie über die Spalte PersonId.
        // Wichtig der letzte Punkt: DeleteBehavior.Restrict heißt "Löschen verbieten".
        // Solange noch eine Adresse an einer Person hängt, lässt sich die Person nicht löschen.
        // Das ist Absicht, damit keine Adressen ohne zugehörige Person übrig bleiben.
        modelBuilder.Entity<Anschrift>()
            .HasOne(a => a.Person)
            .WithMany(p => p.Anschriften)
            .HasForeignKey(a => a.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        // Genau dasselbe für die Telefonnummern: eine Nummer gehört zu einer Person,
        // eine Person hat viele Nummern, und das Löschen ist ebenso geschützt.
        modelBuilder.Entity<Telefonverbindung>()
            .HasOne(t => t.Person)
            .WithMany(p => p.Telefonverbindungen)
            .HasForeignKey(t => t.PersonId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
