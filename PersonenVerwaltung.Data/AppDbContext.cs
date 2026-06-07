using Microsoft.EntityFrameworkCore;
using PersonenVerwaltung.Data.Models;

namespace PersonenVerwaltung.Data;

/// <summary>
/// Entity-Framework-Core-Kontext und zentraler Zugangspunkt der Datenschicht zur
/// PostgreSQL-Datenbank. Kapselt das objektrelationale Mapping sowie die
/// Konfiguration der Entitäten und ihrer Beziehungen.
/// </summary>
/// <remarks>
/// Das physische Schema wird durch <c>database/init.sql</c> definiert; dieser Kontext
/// verwendet keine EF-Migrationen, sondern bildet das bestehende Schema ab. Die
/// Konfiguration in <see cref="OnModelCreating"/> muss daher mit dem SQL-Skript
/// konsistent gehalten werden.
/// </remarks>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Initialisiert den Kontext mit den per Dependency Injection bereitgestellten
    /// Optionen (u. a. Verbindungszeichenfolge und Provider), die in <c>Program.cs</c>
    /// konfiguriert werden.
    /// </summary>
    /// <param name="options">Vom DI-Container gelieferte Kontextoptionen.</param>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    /// <summary>Zugriff auf die Tabelle <c>Person</c>.</summary>
    public DbSet<Person> Personen { get; set; }

    /// <summary>Zugriff auf die Tabelle <c>Anschrift</c>.</summary>
    public DbSet<Anschrift> Anschriften { get; set; }

    /// <summary>Zugriff auf die Tabelle <c>Telefonverbindung</c>.</summary>
    public DbSet<Telefonverbindung> Telefonverbindungen { get; set; }

    /// <summary>
    /// Konfiguriert das Mapping der Entitäten auf das vorgegebene Datenbankschema:
    /// Tabellennamen im Singular sowie die 1:n-Beziehungen mit eingeschränktem
    /// Löschverhalten zur Wahrung der referentiellen Integrität.
    /// </summary>
    /// <param name="modelBuilder">Der von EF Core bereitgestellte Model-Builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Die EF-Core-Pluralisierungskonvention ("People") wird überschrieben, da das
        // Schema aus init.sql Tabellennamen im Singular verwendet.
        modelBuilder.Entity<Person>().ToTable("Person");
        modelBuilder.Entity<Anschrift>().ToTable("Anschrift");
        modelBuilder.Entity<Telefonverbindung>().ToTable("Telefonverbindung");

        // DeleteBehavior.Restrict (ON DELETE NO ACTION) verhindert das Löschen einer
        // Person, solange abhängige Anschriften existieren. Bewusste Entscheidung zur
        // Sicherung der referentiellen Integrität gemäß Anforderung – keine Kaskadierung.
        modelBuilder.Entity<Anschrift>()
            .HasOne(a => a.Person)
            .WithMany(p => p.Anschriften)
            .HasForeignKey(a => a.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        // Identische Beziehungskonfiguration für Telefonverbindungen.
        modelBuilder.Entity<Telefonverbindung>()
            .HasOne(t => t.Person)
            .WithMany(p => p.Telefonverbindungen)
            .HasForeignKey(t => t.PersonId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
