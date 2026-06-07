using PersonenVerwaltung.Data.Models;

namespace PersonenVerwaltung.Data.Repositories;

// Das hier ist ein Interface (eine Art Vertrag).
// Es listet nur AUF, welche Aufgaben es rund um Personen gibt – also was möglich ist –,
// sagt aber noch NICHT, WIE das genau gemacht wird. Das "Wie" steht später in PersonRepository.
// Vorteil: Andere Teile des Programms (z.B. der Controller) müssen nur diesen Vertrag kennen.
// So lässt sich der Daten-Teil später austauschen oder zum Testen ersetzen, ohne dass der Rest
// geändert werden muss.
public interface IPersonRepository
{
    // Holt alle Personen. Gibt man einen Suchtext mit, kommen nur Personen zurück,
    // deren Name oder Vorname diesen Text enthält. Ohne Suchtext kommen alle.
    Task<IEnumerable<Person>> GetAllAsync(string? nameFilter = null);

    // Holt eine einzelne Person anhand ihrer Nummer (Id), inklusive Adressen und Telefonnummern.
    // Gibt es keine Person mit dieser Nummer, kommt "nichts" (null) zurück.
    Task<Person?> GetByIdAsync(int id);

    // Ändert Name und Vorname einer bestimmten Person.
    Task UpdateNameAsync(int id, string name, string vorname);
}
