namespace PersonenVerwaltung.Data.Models;

/// <summary>
/// Domänenentität einer Person und Aggregatwurzel der Datenschicht.
/// Bildet eine Zeile der Tabelle <c>Person</c> als POCO ab; Entity Framework Core
/// übernimmt das Mapping zwischen Objekt und Relation. Die Navigationslisten
/// modellieren die 1:n-Beziehungen zu <see cref="Anschrift"/> und
/// <see cref="Telefonverbindung"/>.
/// </summary>
/// <remarks>
/// Das Schema wird durch <c>database/init.sql</c> vorgegeben, nicht durch EF-Migrationen.
/// Änderungen an dieser Klasse müssen mit dem SQL-Skript abgeglichen werden.
/// </remarks>
public class Person
{
    /// <summary>Primärschlüssel. Wird per EF-Core-Konvention (<c>Id</c>) erkannt.</summary>
    public int Id { get; set; }

    /// <summary>Nachname. Standardwert <see cref="string.Empty"/>, um <c>null</c>-Werte auszuschließen.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Vorname. Standardwert <see cref="string.Empty"/>, um <c>null</c>-Werte auszuschließen.</summary>
    public string Vorname { get; set; } = string.Empty;

    /// <summary>Geburtsdatum ohne Zeitanteil (<see cref="DateOnly"/>).</summary>
    public DateOnly Geburtsdatum { get; set; }

    /// <summary>
    /// Denormalisierte Großschreibung von <see cref="Name"/>. Optional (nullable),
    /// da das SQL-Skript den Wert befüllt und er bei Namensänderungen in
    /// <see cref="PersonenVerwaltung.Data.Repositories.PersonRepository.UpdateNameAsync"/>
    /// synchron gehalten wird.
    /// </summary>
    public string? NameUppercase { get; set; }

    /// <summary>
    /// Anschriften der Person (1:n). Mit leerer Liste vorinitialisiert; befüllt durch
    /// explizites Eager Loading (<c>Include</c>) im Repository.
    /// </summary>
    public ICollection<Anschrift> Anschriften { get; set; } = new List<Anschrift>();

    /// <summary>
    /// Telefonverbindungen der Person (1:n). Mit leerer Liste vorinitialisiert; befüllt
    /// durch explizites Eager Loading (<c>Include</c>) im Repository.
    /// </summary>
    public ICollection<Telefonverbindung> Telefonverbindungen { get; set; } = new List<Telefonverbindung>();
}
