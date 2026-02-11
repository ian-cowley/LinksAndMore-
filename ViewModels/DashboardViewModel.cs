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

    private string? _lastSearchText;
    private bool _isDirty;

    public ObservableCollection<Category> FilteredCategories { get; } = new();

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
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error initializing dashboard: {ex.Message}");
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        Refresh();
    }

    public void Refresh()
    {
        if (!_isDirty && _lastSearchText == SearchText)
        {
            return;
        }

        _lastSearchText = SearchText;
        _isDirty = false;

        foreach (var category in Categories)
        {
            var filtered = category.Items
                .Where(i => string.IsNullOrEmpty(SearchText) ||
                            i.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                            i.Content.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Only update if the list changed to avoid UI flickering/extra work
            if (!category.FilteredItems.SequenceEqual(filtered))
            {
                category.FilteredItems.Clear();
                foreach (var item in filtered)
                {
                    category.FilteredItems.Add(item);
                }
            }
        }

        // Update the list of categories that have at least one visible item
        var visibleCategories = Categories.Where(c => c.FilteredItems.Any()).ToList();
        
        if (!FilteredCategories.SequenceEqual(visibleCategories))
        {
            FilteredCategories.Clear();
            foreach (var category in visibleCategories)
            {
                FilteredCategories.Add(category);
            }
        }
    }

    [RelayCommand]
    private void OpenLink(DashboardItem item)
    {
        if (item.Type == ItemType.Link && !string.IsNullOrEmpty(item.Content))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = item.Content,
                UseShellExecute = true
            });
        }
    }

    [RelayCommand]
    private void CopyToClipboard(DashboardItem item)
    {
        if (!string.IsNullOrEmpty(item.Content))
        {
            System.Windows.Clipboard.SetText(item.Content);
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
