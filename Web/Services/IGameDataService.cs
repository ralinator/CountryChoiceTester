using System.Net.Http.Json;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Web.Data;

namespace Web.Services;

public interface IGameDataService
{
    Task<GameData> GetAsync();
}

public class GameDataService : IGameDataService
{
    private readonly HttpClient _httpClient;
    private GameData? _gameData;

    public GameDataService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GameData> GetAsync()
    {
        if (_gameData is null)
        {
            var jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            jsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            _gameData = await _httpClient.GetFromJsonAsync<GameData>("gamedata.json", jsonSerializerOptions);
            if (_gameData is null)
            {
                throw new Exception("Game Data Not Found");
            }
        }
        return _gameData;
    }
}
