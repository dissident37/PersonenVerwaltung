using PersonenVerwaltung.Data.Models;

namespace PersonenVerwaltung.Data.Repositories;

/// <summary>
/// Abstraktion des Datenzugriffs auf <see cref="Person"/>-Entitäten (Repository-Pattern).
/// Definiert den Vertrag der Datenschicht, unabhängig von der konkreten
/// Persistenztechnologie.
/// </summary>
/// <remarks>
/// Konsumenten (z. B. der API-Controller) hängen ausschließlich von dieser Abstraktion ab.
/// Das entkoppelt die darüberliegenden Schichten von Entity Framework Core, ermöglicht den
/// Austausch der Implementierung und erleichtert die Testbarkeit durch Mocking.
/// </remarks>
public interface IPersonRepository
{
    /// <summary>
    /// Lädt alle Personen, optional gefiltert nach einem Suchbegriff.
    /// </summary>
    /// <param name="nameFilter">
    /// Optionaler Suchbegriff; bei Angabe werden Personen geliefert, deren Name oder
    /// Vorname den Begriff enthält (case-insensitive). <c>null</c> oder leer = kein Filter.
    /// </param>
    /// <returns>Die nach Name und Vorname sortierte Treffermenge.</returns>
    Task<IEnumerable<Person>> GetAllAsync(string? nameFilter = null);

    /// <summary>
    /// Lädt eine einzelne Person samt Anschriften und Telefonverbindungen.
    /// </summary>
    /// <param name="id">Primärschlüssel der gesuchten Person.</param>
    /// <returns>Die Person inklusive Detaildaten oder <c>null</c>, wenn keine existiert.</returns>
    Task<Person?> GetByIdAsync(int id);

    /// <summary>
    /// Aktualisiert Name und Vorname einer Person.
    /// </summary>
    /// <param name="id">Primärschlüssel der zu ändernden Person.</param>
    /// <param name="name">Neuer Nachname.</param>
    /// <param name="vorname">Neuer Vorname.</param>
    Task UpdateNameAsync(int id, string name, string vorname);
}
