using System.IO;
using System.Text.Json;
using System.Collections.ObjectModel;
using LinksAndMore.Models;

namespace LinksAndMore.Services;

public interface IDataService
{
    Task<ObservableCollection<Category>> LoadDataAsync();
    Task SaveDataAsync(ObservableCollection<Category> categories);
}

public class DataService : IDataService
{
    private static readonly string DataFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
        "LinksAndMore", "links.json");

    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public async Task<ObservableCollection<Category>> LoadDataAsync()
    {
        if (!File.Exists(DataFilePath))
        {
            return new ObservableCollection<Category>();
        }

        using FileStream openStream = File.OpenRead(DataFilePath);
        var data = await JsonSerializer.DeserializeAsync<List<Category>>(openStream, _options);
        return new ObservableCollection<Category>(data ?? new List<Category>());
    }

    public async Task SaveDataAsync(ObservableCollection<Category> categories)
    {
        var directory = Path.GetDirectoryName(DataFilePath);
        if (directory != null && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using FileStream createStream = File.Create(DataFilePath);
        await JsonSerializer.SerializeAsync(createStream, categories, _options);
    }
}
