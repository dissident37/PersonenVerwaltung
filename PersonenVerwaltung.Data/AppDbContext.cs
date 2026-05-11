using Microsoft.EntityFrameworkCore;
using PersonenVerwaltung.Data.Models;

namespace PersonenVerwaltung.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Person> Personen { get; set; }
    public DbSet<Anschrift> Anschriften { get; set; }
    public DbSet<Telefonverbindung> Telefonverbindungen { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Tabellennamen entsprechen den SQL-Tabellen
        modelBuilder.Entity<Person>().ToTable("Person");
        modelBuilder.Entity<Anschrift>().ToTable("Anschrift");
        modelBuilder.Entity<Telefonverbindung>().ToTable("Telefonverbindung");

        // Fremdschluessel: kein Loeschen (referentielle Integritaet)
        modelBuilder.Entity<Anschrift>()
            .HasOne(a => a.Person)
            .WithMany(p => p.Anschriften)
            .HasForeignKey(a => a.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Telefonverbindung>()
            .HasOne(t => t.Person)
            .WithMany(p => p.Telefonverbindungen)
            .HasForeignKey(t => t.PersonId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
