namespace Web.Data;

public class GameData
{
    public GameData(List<Country> countries, List<Continent> continents)
    {
        Countries = countries;
        Continents = continents;
    }

    public List<Country> Countries { get; set; }
    public List<Continent> Continents { get; set; }
}
