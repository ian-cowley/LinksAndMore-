using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LinksAndMore.Models;
using LinksAndMore.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Wpf.Ui.Appearance;

namespace LinksAndMore.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IDataService _dataService;

    [ObservableProperty]
    private string _appVersion = "1.0.0";

    [ObservableProperty]
    private string _dataFilePath;

    [ObservableProperty]
    private Wpf.Ui.Appearance.ApplicationTheme _selectedTheme;

    [ObservableProperty]
    private ObservableCollection<Category> _categories;

    [ObservableProperty]
    private string _newCategoryName = string.Empty;

    public SettingsViewModel(IDataService dataService)
    {
        _dataService = dataService;
        _dataFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
            "LinksAndMore", "links.json");
        
        _selectedTheme = ApplicationThemeManager.GetAppTheme();

        _categories = new ObservableCollection<Category>();
        _ = LoadData();
    }

    private async Task LoadData()
    {
        try
        {
            var loadedCategories = await _dataService.LoadDataAsync();
            foreach (var category in loadedCategories)
            {
                Categories.Add(category);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading settings data: {ex.Message}");
        }
    }

    [RelayCommand]
    private void ChangeTheme(Wpf.Ui.Appearance.ApplicationTheme theme)
    {
        SelectedTheme = theme;
        if (theme == Wpf.Ui.Appearance.ApplicationTheme.Unknown)
        {
            ApplicationThemeManager.ApplySystemTheme();
        }
        else
        {
            ApplicationThemeManager.Apply(theme);
        }
    }

    [RelayCommand]
    private void OpenDataFolder()
    {
        try
        {
            var directory = Path.GetDirectoryName(DataFilePath);
            if (directory != null && Directory.Exists(directory))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = directory,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error opening data folder: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task AddCategory()
    {
        if (string.IsNullOrWhiteSpace(NewCategoryName)) return;

        var category = new Category { Name = NewCategoryName };
        Categories.Add(category);
        NewCategoryName = string.Empty;
        await SaveData();
    }

    [RelayCommand]
    private async Task DeleteCategory(Category category)
    {
        if (category == null) return;
        
        // If it has items, we might want to move them or warn. 
        // For now, let's just delete it to keep it simple, or move items to the first category if it exists.
        if (Categories.Count > 1)
        {
            var target = Categories.FirstOrDefault(c => c != category);
            if (target != null)
            {
                foreach (var item in category.Items.ToList())
                {
                    target.Items.Add(item);
                }
            }
        }
        
        Categories.Remove(category);
        await SaveData();
    }

    [RelayCommand]
    private async Task SaveData()
    {
        await _dataService.SaveDataAsync(Categories);
    }
}
