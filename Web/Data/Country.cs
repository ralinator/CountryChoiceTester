namespace Web.Data;

public class Country
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Continent { get; set; } = null!;
    public string? AlternativeName { get; set; }
}
