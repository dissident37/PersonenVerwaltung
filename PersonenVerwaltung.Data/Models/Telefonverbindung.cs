namespace PersonenVerwaltung.Data.Models;

public class Telefonverbindung
{
    public int Id { get; set; }
    public int PersonId { get; set; }
    public string Nummer { get; set; } = string.Empty;

    public Person Person { get; set; } = null!;
}
