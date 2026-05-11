namespace PersonenVerwaltung.Data.Models;

public class Person
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Vorname { get; set; } = string.Empty;
    public DateOnly Geburtsdatum { get; set; }
    public string? NameUppercase { get; set; }

    public ICollection<Anschrift> Anschriften { get; set; } = new List<Anschrift>();
    public ICollection<Telefonverbindung> Telefonverbindungen { get; set; } = new List<Telefonverbindung>();
}
