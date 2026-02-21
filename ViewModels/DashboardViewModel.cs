using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using LinksAndMore.Models;
using LinksAndMore.Services;
using System.Diagnostics;

namespace LinksAndMore.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IDataService _dataService;

    [ObservableProperty]
    private ObservableCollection<Category> _categories = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isSemanticSearchEnabled = App.IsSemanticSearchEnabled;

    private string? _lastSearchText;
    private bool? _lastSemanticState;
    private bool _isDirty;

    [ObservableProperty]
    private ObservableCollection<Category> _filteredCategories = new();

    [ObservableProperty]
    private string _indexingProgressText = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    public DashboardViewModel(IDataService dataService)
    {
        _dataService = dataService;
        Categories.CollectionChanged += OnCategoriesCollectionChanged;
        _ = InitializeAsync();
    }

    partial void OnCategoriesChanged(ObservableCollection<Category> value)
    {
        Categories.CollectionChanged -= OnCategoriesCollectionChanged;
        Categories.CollectionChanged += OnCategoriesCollectionChanged;
        AttachCategoryHandlers(value);
        _isDirty = true;
        Refresh();
    }

    private void OnCategoriesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            foreach (Category oldCategory in e.OldItems)
            {
                oldCategory.Items.CollectionChanged -= OnItemsChanged;
            }
        }

        if (e.NewItems != null)
        {
            foreach (Category newCategory in e.NewItems)
            {
                newCategory.Items.CollectionChanged += OnItemsChanged;
            }
        }

        _isDirty = true;
        Refresh();
    }

    private void AttachCategoryHandlers(IEnumerable<Category> categories)
    {
        foreach (var category in categories)
        {
            category.Items.CollectionChanged -= OnItemsChanged;
            category.Items.CollectionChanged += OnItemsChanged;
        }
    }

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        _isDirty = true;
        Refresh();
    }

    private async Task InitializeAsync()
    {
        try
        {
            Categories = await _dataService.LoadDataAsync();
            Refresh();

            // Handle AI Init Pipeline
            if (!ModelDownloadService.IsModelDownloaded())
            {
                IsBusy = true;
                IndexingProgressText = "Downloading AI Model (90MB)...";
                var progress = new Progress<double>(p => 
                {
                    IndexingProgressText = $"Downloading AI Model... {(int)(p * 100)}%";
                });
                await ModelDownloadService.DownloadModelAsync(progress);
                
                // Re-init SemanticEngine now that it's downloaded
                App.SemanticEngine.GenerateEmbedding("warmup");
            }

            var indexer = new BackgroundIndexer(_dataService, App.SemanticEngine);
            indexer.ProgressChanged += (s, e) =>
            {
                if (e.Processed < e.Total)
                {
                    IsBusy = true;
                    IndexingProgressText = $"Updating AI Index... {e.Processed}/{e.Total}";
                }
                else
                {
                    IsBusy = false;
                    IndexingProgressText = string.Empty;
                }
            };
            
            await indexer.StartIndexingAsync();
            IsBusy = false;
        }
        catch (Exception ex)
        {
            IsBusy = false;
            Debug.WriteLine($"Error initializing dashboard: {ex.Message}");
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        Refresh();
    }

    partial void OnIsSemanticSearchEnabledChanged(bool value)
    {
        App.IsSemanticSearchEnabled = value;
        Refresh();
    }

    public void Refresh()
    {
        if (!_isDirty && _lastSearchText == SearchText && _lastSemanticState == IsSemanticSearchEnabled)
        {
            return;
        }

        _lastSearchText = SearchText;
        _lastSemanticState = IsSemanticSearchEnabled;
        _isDirty = false;

        float[]? queryVector = null;
        if (IsSemanticSearchEnabled && !string.IsNullOrWhiteSpace(SearchText))
        {
            queryVector = App.SemanticEngine.GenerateEmbedding(SearchText);
        }

        foreach (var category in Categories)
        {
            List<DashboardItem> filtered;

            if (IsSemanticSearchEnabled)
            {
                var scoredItems = category.Items.Select(i => 
                {
                    float score = 0;
                    if (string.IsNullOrWhiteSpace(SearchText) || queryVector == null)
                    {
                        score = 1;
                        i.RelevanceString = "AI Active";
                    }
                    else if (i.VectorEmbedding != null)
                    {
                        score = App.SemanticEngine.CalculateCosineSimilarity(queryVector, i.VectorEmbedding);
                        i.RelevanceString = score > 0 ? $"{(int)(score * 100)}% Match" : string.Empty;
                    }
                    else
                    {
                        bool isTextMatch = i.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                                           i.Content.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
                        score = isTextMatch ? 1 : 0;
                        i.RelevanceString = isTextMatch ? "Text Match" : string.Empty;
                    }
                    
                    i.RelevanceScore = score;
                    return new { Item = i, Score = score };
                })
                .Where(x => string.IsNullOrWhiteSpace(SearchText) || x.Score > 0.3f) // Filter low relevance
                .OrderByDescending(x => x.Score)
                .Select(x => x.Item)
                .ToList();

                filtered = scoredItems;
            }
            else
            {
                filtered = category.Items
                    .Where(i => string.IsNullOrEmpty(SearchText) ||
                                i.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                                i.Content.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                    .Select(i => 
                    {
                        i.RelevanceScore = 0;
                        i.RelevanceString = string.Empty;
                        return i;
                    })
                    .ToList();
            }

            if (!category.FilteredItems.SequenceEqual(filtered))
            {
                category.FilteredItems = new ObservableCollection<DashboardItem>(filtered);
            }
        }

        var visibleCategories = Categories.Where(c => c.FilteredItems.Any()).ToList();
        
        if (!FilteredCategories.SequenceEqual(visibleCategories))
        {
            FilteredCategories = new ObservableCollection<Category>(visibleCategories);
        }
    }

    [RelayCommand]
    private void OpenLink(DashboardItem item)
    {
        if (item.Type == ItemType.Link && !string.IsNullOrEmpty(item.Content))
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = item.Content,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error opening link: {ex.Message}");
            }
        }
    }

    [RelayCommand]
    private async Task UnlockPassword(DashboardItem item)
    {
        if (item.Type == ItemType.Password && item.IsLocked)
        {
            var success = await App.BiometricService.AuthenticateUserAsync("Please authenticate to show your password.");
            if (success)
            {
                item.DecryptedContent = App.SecurityService.Decrypt(item.Content);
                item.IsLocked = false;
                
                // Auto-lock after 30 seconds
                _ = Task.Delay(30000).ContinueWith(_ => 
                {
                    item.IsLocked = true;
                    item.DecryptedContent = string.Empty;
                });
            }
        }
    }

    [RelayCommand]
    private void CopyToClipboard(DashboardItem item)
    {
        string textToCopy = item.Type == ItemType.Password ? 
            (item.IsLocked ? string.Empty : item.DecryptedContent) : 
            item.Content;

        if (!string.IsNullOrEmpty(textToCopy))
        {
            System.Windows.Clipboard.SetText(textToCopy);
        }
    }

    [RelayCommand]
    private async Task DeleteItem(DashboardItem item)
    {
        foreach (var category in Categories)
        {
            if (category.Items.Contains(item))
            {
                category.Items.Remove(item);
                break;
            }
        }
        await SaveData();
        Refresh();
    }

    [RelayCommand]
    private async Task SaveData()
    {
        await _dataService.SaveDataAsync(Categories);
    }
}
