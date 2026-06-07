namespace PersonenVerwaltung.Data.Models;

// Diese Klasse beschreibt eine Anschrift (also eine Adresse) einer Person.
// Jede Adresse gehört zu genau einer Person. Eine Person kann mehrere Adressen haben.
public class Anschrift
{
    // Eindeutige Nummer dieser Adresse.
    public int Id { get; set; }

    // Nummer der Person, zu der diese Adresse gehört (verweist auf Person.Id).
    public int PersonId { get; set; }

    // Postleitzahl. Als Text gespeichert, damit eine führende Null (z.B. "01067") nicht verloren geht.
    public string Postleitzahl { get; set; } = string.Empty;

    // Ort / Stadt.
    public string Ort { get; set; } = string.Empty;

    // Straße.
    public string Strasse { get; set; } = string.Empty;

    // Hausnummer. Als Text, weil auch Zusätze möglich sind (z.B. "12a").
    public string Hausnummer { get; set; } = string.Empty;

    // Verweis zurück auf die Person, zu der diese Adresse gehört.
    // Wird beim Laden aus der Datenbank automatisch gesetzt.
    public Person Person { get; set; } = null!;
}
