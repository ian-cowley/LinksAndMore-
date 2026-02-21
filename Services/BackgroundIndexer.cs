using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace LinksAndMore.Services;

public interface IBackgroundIndexer
{
    Task StartIndexingAsync();
    event EventHandler<IndexerProgressEventArgs>? ProgressChanged;
}

public class IndexerProgressEventArgs : EventArgs
{
    public int Processed { get; set; }
    public int Total { get; set; }
}

public class BackgroundIndexer : IBackgroundIndexer
{
    private readonly IDataService _dataService;
    private readonly ISemanticEngine _semanticEngine;

    public event EventHandler<IndexerProgressEventArgs>? ProgressChanged;

    public BackgroundIndexer(IDataService dataService, ISemanticEngine semanticEngine)
    {
        _dataService = dataService;
        _semanticEngine = semanticEngine;
    }

    public async Task StartIndexingAsync()
    {
        if (!_semanticEngine.IsLoaded)
        {
            return;
        }

        var categories = await _dataService.LoadDataAsync();
        var allItems = categories.SelectMany(c => c.Items).ToList();
        var itemsToIndex = allItems.Where(i => i.VectorEmbedding == null).ToList();

        if (itemsToIndex.Count == 0) return;

        int processedCount = 0;
        
        // Report initial progress
        ProgressChanged?.Invoke(this, new IndexerProgressEventArgs 
        { 
            Processed = processedCount, 
            Total = itemsToIndex.Count 
        });

        // Use Task.Run so we don't block the UI thread
        await Task.Run(() =>
        {
            foreach (var item in itemsToIndex)
            {
                string textToEmbed = $"Title: {item.Title}. Description: {item.Description ?? string.Empty}.";
                item.VectorEmbedding = _semanticEngine.GenerateEmbedding(textToEmbed);
                
                processedCount++;
                ProgressChanged?.Invoke(this, new IndexerProgressEventArgs 
                { 
                    Processed = processedCount, 
                    Total = itemsToIndex.Count 
                });
            }
        });

        // Save after indexing
        await _dataService.SaveDataAsync(categories);
    }
}
