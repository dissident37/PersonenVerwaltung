using Microsoft.EntityFrameworkCore;
using PersonenVerwaltung.Data.Models;

namespace PersonenVerwaltung.Data.Repositories;

public class PersonRepository : IPersonRepository
{
    private readonly AppDbContext _db;

    public PersonRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Person>> GetAllAsync(string? nameFilter = null)
    {
        var query = _db.Personen.AsQueryable();

        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            var filter = nameFilter.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(filter) ||
                p.Vorname.ToLower().Contains(filter));
        }

        return await query.OrderBy(p => p.Name).ThenBy(p => p.Vorname).ToListAsync();
    }

    public async Task<Person?> GetByIdAsync(int id)
    {
        return await _db.Personen
            .Include(p => p.Anschriften)
            .Include(p => p.Telefonverbindungen)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task UpdateNameAsync(int id, string name, string vorname)
    {
        var person = await _db.Personen.FindAsync(id);
        if (person == null) return;

        person.Name = name;
        person.Vorname = vorname;
        person.NameUppercase = name.ToUpper();
        await _db.SaveChangesAsync();
    }
}
