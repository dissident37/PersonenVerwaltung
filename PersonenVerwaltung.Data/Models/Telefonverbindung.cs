namespace PersonenVerwaltung.Data.Models;

// Diese Klasse beschreibt eine Telefonnummer einer Person.
// Jede Nummer gehört zu genau einer Person. Eine Person kann mehrere Nummern haben.
public class Telefonverbindung
{
    // Eindeutige Nummer dieses Eintrags.
    public int Id { get; set; }

    // Nummer der Person, zu der diese Telefonnummer gehört (verweist auf Person.Id).
    public int PersonId { get; set; }

    // Die Telefonnummer selbst. Als Text gespeichert (wegen Vorwahl, Sonderzeichen und führender Null).
    public string Nummer { get; set; } = string.Empty;

    // Verweis zurück auf die Person, zu der diese Nummer gehört.
    // Wird beim Laden aus der Datenbank automatisch gesetzt.
    public Person Person { get; set; } = null!;
}
