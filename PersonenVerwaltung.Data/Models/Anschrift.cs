namespace PersonenVerwaltung.Data.Models;

public class Anschrift
{
    public int Id { get; set; }
    public int PersonId { get; set; }
    public string Postleitzahl { get; set; } = string.Empty;
    public string Ort { get; set; } = string.Empty;
    public string Strasse { get; set; } = string.Empty;
    public string Hausnummer { get; set; } = string.Empty;

    public Person Person { get; set; } = null!;
}
