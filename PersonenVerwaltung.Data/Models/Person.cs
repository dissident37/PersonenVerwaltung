namespace PersonenVerwaltung.Data.Models;

// Diese Klasse beschreibt eine einzelne Person mit allen ihren Daten.
// Jede Person in der Datenbank-Tabelle "Person" wird im Programm zu so einem Person-Objekt.
// Eine Person kann mehrere Anschriften (Adressen) und mehrere Telefonnummern haben –
// die hängen unten als Listen dran.
public class Person
{
    // Eindeutige Nummer dieser Person. Damit lässt sich jede Person sicher wiederfinden.
    public int Id { get; set; }

    // Nachname. Startet als leerer Text statt als "nichts" (null), damit er nie unbefüllt ist.
    public string Name { get; set; } = string.Empty;

    // Vorname. Startet ebenfalls als leerer Text.
    public string Vorname { get; set; } = string.Empty;

    // Geburtsdatum – nur das Datum, ohne Uhrzeit (z.B. 06.06.1990).
    public DateOnly Geburtsdatum { get; set; }

    // Der Nachname noch einmal in Großbuchstaben. Diese Zweitkopie wird absichtlich gehalten;
    // das Fragezeichen bedeutet: das Feld darf auch leer (null) sein.
    // Es wird beim Ändern des Namens immer mit aktualisiert (siehe PersonRepository).
    public string? NameUppercase { get; set; }

    // Liste aller Anschriften (Adressen) dieser Person. Startet als leere Liste.
    // Eine Person kann viele Anschriften haben.
    public ICollection<Anschrift> Anschriften { get; set; } = new List<Anschrift>();

    // Liste aller Telefonnummern dieser Person. Startet als leere Liste.
    // Eine Person kann viele Telefonnummern haben.
    public ICollection<Telefonverbindung> Telefonverbindungen { get; set; } = new List<Telefonverbindung>();
}
