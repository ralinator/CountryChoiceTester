namespace Web.Data;

public class GameRecord
{
    public int Id { get; set; }
    public DateTime Date { get; set; } = DateTime.Today;
    public int NumberOfAttemptsRequired { get; set; }
}
