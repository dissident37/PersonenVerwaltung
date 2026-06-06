using PersonenVerwaltung.Data.Models;

namespace PersonenVerwaltung.Data.Repositories;

// WAS IST DAS: Ein Interface (Vertrag) für den Daten-Zugriff auf Personen.
// WARUM BRAUCHEN WIR DAS: Es legt fest, WELCHE Methoden es zum Lesen/Ändern von Personen gibt,
//                         sagt aber NICHT WIE sie funktionieren. Das "Wie" steht in PersonRepository.
// IM VORSTELLUNGSGESPRÄCH SAGEN:
//   "Ich arbeite mit dem Repository-Pattern. Das Interface ist der Vertrag, die Klasse die Umsetzung.
//    Vorteil: Mein API-Controller hängt nur am Interface, nicht an EF Core. So kann ich die Datenschicht
//    austauschen oder im Test durch eine Attrappe (Mock) ersetzen, ohne den Controller zu ändern.
//    Das nennt man 'lose Kopplung'."
//
// Interface = Vertrag: Wer dieses Interface umsetzt (implementiert), MUSS genau diese Methoden anbieten.
// Konvention in C#: Interface-Namen beginnen mit "I" (I = Interface).
public interface IPersonRepository
{
    // Task<...> = die Methode arbeitet asynchron (async): Sie kann etwas tun, das dauert (DB-Abfrage),
    //             ohne den ganzen Server zu blockieren. Task<T> = "ein Ergebnis vom Typ T, das später kommt".
    // IEnumerable<Person> = "eine Liste/Aufzählung von Personen" (zum Durchlaufen, read-only).
    // "string? nameFilter = null" = optionaler Such-Text; "?" heißt: darf null sein; "= null" = Standardwert.
    Task<IEnumerable<Person>> GetAllAsync(string? nameFilter = null);   // alle Personen holen, optional gefiltert.

    // Person? = liefert eine Person ODER null, falls die Id nicht existiert. Das "?" zwingt den Aufrufer,
    //           den Fall "nicht gefunden" zu bedenken (Schutz vor NullReferenceException).
    Task<Person?> GetByIdAsync(int id);                                 // eine einzelne Person per Id holen (mit Details).

    // Ändert nur Name + Vorname einer Person. Gibt nichts zurück (Task ohne <T>) = "nur erledigen, kein Ergebnis".
    Task UpdateNameAsync(int id, string name, string vorname);
}
