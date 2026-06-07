namespace PersonenVerwaltung.Data.Models;

/// <summary>
/// Domänenentität einer Telefonverbindung. Untergeordneter Teil des Aggregats
/// <see cref="Person"/> (1:n) und Abbild einer Zeile der Tabelle <c>Telefonverbindung</c>.
/// </summary>
public class Telefonverbindung
{
    /// <summary>Primärschlüssel.</summary>
    public int Id { get; set; }

    /// <summary>Fremdschlüssel auf <see cref="Person.Id"/>.</summary>
    public int PersonId { get; set; }

    /// <summary>Telefonnummer. Als Zeichenkette gehalten (Vorwahl, Sonderzeichen, führende Null).</summary>
    public string Nummer { get; set; } = string.Empty;

    /// <summary>
    /// Navigationseigenschaft zur übergeordneten Person. <c>null!</c> unterdrückt die
    /// Nullable-Warnung, da EF Core die Referenz beim Laden zuverlässig setzt.
    /// </summary>
    public Person Person { get; set; } = null!;
}
