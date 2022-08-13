namespace Web.Data;

public class GameState
{
    public int Id { get; set; }
    public string State { get; set; } = "";
    public DateTime Date { get; set; } = DateTime.Today;
}
