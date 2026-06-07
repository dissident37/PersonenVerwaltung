namespace PersonenVerwaltung.Data.Models;

/// <summary>
/// Domänenentität einer Anschrift. Untergeordneter Teil des Aggregats
/// <see cref="Person"/> (1:n) und Abbild einer Zeile der Tabelle <c>Anschrift</c>.
/// </summary>
public class Anschrift
{
    /// <summary>Primärschlüssel.</summary>
    public int Id { get; set; }

    /// <summary>Fremdschlüssel auf <see cref="Person.Id"/>.</summary>
    public int PersonId { get; set; }

    /// <summary>Postleitzahl. Als Zeichenkette gehalten, um führende Nullen zu bewahren.</summary>
    public string Postleitzahl { get; set; } = string.Empty;

    /// <summary>Ort.</summary>
    public string Ort { get; set; } = string.Empty;

    /// <summary>Straße.</summary>
    public string Strasse { get; set; } = string.Empty;

    /// <summary>Hausnummer. Als Zeichenkette gehalten (Zusätze wie "12a" möglich).</summary>
    public string Hausnummer { get; set; } = string.Empty;

    /// <summary>
    /// Navigationseigenschaft zur übergeordneten Person. <c>null!</c> unterdrückt die
    /// Nullable-Warnung, da EF Core die Referenz beim Laden zuverlässig setzt.
    /// </summary>
    public Person Person { get; set; } = null!;
}
