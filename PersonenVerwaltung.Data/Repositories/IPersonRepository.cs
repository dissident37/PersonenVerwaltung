using PersonenVerwaltung.Data.Models;

namespace PersonenVerwaltung.Data.Repositories;

public interface IPersonRepository
{
    Task<IEnumerable<Person>> GetAllAsync(string? nameFilter = null);
    Task<Person?> GetByIdAsync(int id);
    Task UpdateNameAsync(int id, string name, string vorname);
}
