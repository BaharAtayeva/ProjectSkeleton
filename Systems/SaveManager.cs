using System.Text.Json;
using TheAdventure.Exceptions;

namespace TheAdventure.Systems;

public record HighScoreEntry(string Date, int Score, int Floor, int Gold);

public class SaveManager
{
    private readonly string _savePath;
    private static readonly JsonSerializerOptions _options = new() { WriteIndented = true };

    public SaveManager()
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string dir = Path.Combine(appData, "TheAdventure");
        Directory.CreateDirectory(dir);
        _savePath = Path.Combine(dir, "highscores.json");
    }

    public async Task<List<HighScoreEntry>> LoadHighScoresAsync()
    {
        try
        {
            if (!File.Exists(_savePath)) return new List<HighScoreEntry>();
            string json = await File.ReadAllTextAsync(_savePath);
            return JsonSerializer.Deserialize<List<HighScoreEntry>>(json) ?? new List<HighScoreEntry>();
        }
        catch (Exception ex)
        {
            throw new SaveLoadException("Failed to load high scores.", ex);
        }
    }

    public async Task SaveHighScoreAsync(int score, int floor, int gold)
    {
        try
        {
            var scores = await LoadHighScoresAsync();
            scores.Add(new HighScoreEntry(DateTime.Now.ToString("yyyy-MM-dd HH:mm"), score, floor, gold));

            // LINQ: keep top 10 sorted by score
            scores = scores.OrderByDescending(s => s.Score).Take(10).ToList();

            string json = JsonSerializer.Serialize(scores, _options);
            await File.WriteAllTextAsync(_savePath, json);
        }
        catch (SaveLoadException) { throw; }
        catch (Exception ex)
        {
            throw new SaveLoadException("Failed to save high score.", ex);
        }
    }
}