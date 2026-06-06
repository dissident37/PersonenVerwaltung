// using = "ich möchte Code aus diesem Bereich (Namespace) benutzen, ohne jedes Mal den vollen Namen zu schreiben".
using Microsoft.EntityFrameworkCore;   // Entity Framework Core (EF Core) = das ORM, das C#-Objekte <-> Datenbank-Tabellen übersetzt.
using PersonenVerwaltung.Data.Models;  // unsere eigenen Modell-Klassen: Person, Anschrift, Telefonverbindung.

// namespace = "Ordner/Schublade" für Klassen, damit Namen sich nicht überschneiden.
namespace PersonenVerwaltung.Data;

// WAS IST DAS: Die zentrale Datenbank-Klasse (der "DbContext") von Entity Framework Core.
// WARUM BRAUCHEN WIR DAS: Sie ist die Brücke zwischen unserem C#-Code und der PostgreSQL-Datenbank;
//                         über sie lesen und schreiben wir Daten, ohne selbst SQL schreiben zu müssen.
// IM VORSTELLUNGSGESPRÄCH SAGEN:
//   "Der AppDbContext ist mein Zugang zur Datenbank. EF Core ist ein ORM (Object-Relational Mapper):
//    Es bildet jede Tabelle als C#-Klasse ab. Ich arbeite mit normalen Objekten, und EF Core erzeugt
//    daraus im Hintergrund das passende SQL."
public class AppDbContext : DbContext   // ": DbContext" = wir erben von DbContext, der Basis-Klasse von EF Core. Erben = wir bekommen alle Fähigkeiten von DbContext geschenkt.
{
    // Konstruktor = Methode, die beim Erzeugen des Objekts läuft. Hier werden die Einstellungen
    // (z.B. welche Datenbank, welche Verbindungszeichenfolge) hineingereicht.
    // "options" kommt von außen (Dependency Injection in Program.cs) -> "base(options)" gibt sie an DbContext weiter.
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // DbSet<T> = "eine Tabelle als Liste in C#". Jeder DbSet entspricht einer Tabelle in der DB.
    // Über diese Eigenschaften stellen wir Abfragen (z.B. _db.Personen.Where(...)).
    // { get; set; } = automatische Eigenschaft (Property): EF Core befüllt sie selbst.
    public DbSet<Person> Personen { get; set; }                          // Tabelle "Person"
    public DbSet<Anschrift> Anschriften { get; set; }                    // Tabelle "Anschrift"
    public DbSet<Telefonverbindung> Telefonverbindungen { get; set; }    // Tabelle "Telefonverbindung"

    // WAS MACHT DIESE METHODE: Sie konfiguriert, wie die C#-Klassen genau auf die DB-Tabellen abgebildet werden.
    // WARUM: Unsere Tabellen heißen im Singular (Person, Anschrift, ...) und es gibt feste Regeln für
    //        Fremdschlüssel. Das müssen wir EF Core hier explizit mitteilen.
    // "override" = wir überschreiben eine Methode, die EF Core uns vorgibt, mit unserer eigenen Variante.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Standardmäßig würde EF Core Tabellen im Plural erwarten ("People"). Unsere SQL-Datei
        // (database/init.sql) benutzt aber Singular. Darum mappen wir die Namen hier von Hand.
        // Wichtig: Die DB-Struktur kommt aus init.sql, NICHT aus EF-Migrationen -> wir müssen sie passend abbilden.
        modelBuilder.Entity<Person>().ToTable("Person");
        modelBuilder.Entity<Anschrift>().ToTable("Anschrift");
        modelBuilder.Entity<Telefonverbindung>().ToTable("Telefonverbindung");

        // Fremdschlüssel (Foreign Key) = Verweis von einer Tabelle auf eine andere.
        // Eine Anschrift "gehört" zu genau einer Person (PersonId zeigt auf Person.Id).
        // DeleteBehavior.Restrict = "Verbieten zu löschen, solange noch Anschriften dranhängen"
        //   -> referentielle Integrität: keine verwaisten Datensätze. Genau so eine Anforderung der Aufgabe.
        modelBuilder.Entity<Anschrift>()
            .HasOne(a => a.Person)                 // eine Anschrift hat EINE Person ...
            .WithMany(p => p.Anschriften)          // ... und eine Person hat VIELE Anschriften (1:n-Beziehung).
            .HasForeignKey(a => a.PersonId)        // die Spalte, die die beiden verbindet.
            .OnDelete(DeleteBehavior.Restrict);    // Löschen der Person blockieren, falls noch Anschriften existieren.

        // Gleiches Prinzip für Telefonverbindungen: 1 Person : n Telefonnummern, Löschen geschützt.
        modelBuilder.Entity<Telefonverbindung>()
            .HasOne(t => t.Person)
            .WithMany(p => p.Telefonverbindungen)
            .HasForeignKey(t => t.PersonId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
