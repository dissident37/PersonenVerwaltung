namespace PersonenVerwaltung.Data.Models;

// WAS IST DAS: Eine "Entity"-Klasse – das C#-Abbild einer Zeile in der Tabelle "Person".
// WARUM BRAUCHEN WIR DAS: EF Core verwandelt jede DB-Zeile in so ein Objekt (und umgekehrt).
//                         Statt mit SQL-Zeilen arbeiten wir im Code mit sauberen Person-Objekten.
// IM VORSTELLUNGSGESPRÄCH SAGEN:
//   "Das ist ein POCO – ein 'Plain Old CLR Object', also eine ganz normale C#-Klasse ohne Framework-Ballast.
//    Jede Eigenschaft (Property) entspricht einer Spalte. Die Property 'Id' wird von EF Core automatisch als
//    Primärschlüssel erkannt (Konvention: 'Id' oder 'KlassennameId'). Die Listen am Ende sind die
//    Navigations-Eigenschaften für die 1:n-Beziehungen zu Anschriften und Telefonverbindungen."
public class Person
{
    // Primärschlüssel = eindeutige Kennung jeder Person. Heißt "Id" -> EF Core erkennt das automatisch.
    public int Id { get; set; }

    // string = Text. "= string.Empty" = Startwert ist ein leerer Text ("") statt null.
    // Das verhindert, dass die Eigenschaft versehentlich null ist (sicherer im Umgang).
    public string Name { get; set; } = string.Empty;
    public string Vorname { get; set; } = string.Empty;

    // DateOnly = nur ein Datum OHNE Uhrzeit (z.B. 06.06.1990). Passt zu einem Geburtsdatum.
    public DateOnly Geburtsdatum { get; set; }

    // "string?" mit Fragezeichen = darf null sein (nullable). Diese Spalte ist optional.
    // NameUppercase = der Name in Großbuchstaben; eine bewusst doppelt gehaltene (denormalisierte) Kopie.
    // Wird in PersonRepository.UpdateNameAsync immer mit name.ToUpper() synchron gehalten.
    public string? NameUppercase { get; set; }

    // Navigations-Eigenschaften = der Weg von einer Person zu ihren verknüpften Datensätzen.
    // ICollection<T> = "eine Sammlung von T-Objekten". "= new List<...>()" = startet als leere Liste,
    //                  damit man sofort hinzufügen kann, ohne erst auf null prüfen zu müssen.
    // EF Core befüllt diese Listen, wenn wir im Repository .Include(...) verwenden.
    public ICollection<Anschrift> Anschriften { get; set; } = new List<Anschrift>();                 // 1 Person : n Anschriften
    public ICollection<Telefonverbindung> Telefonverbindungen { get; set; } = new List<Telefonverbindung>();   // 1 Person : n Telefonnummern
}
