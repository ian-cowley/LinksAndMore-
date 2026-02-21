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
    private string _appVersion = "2.0.0";

    [ObservableProperty]
    private string _dataFilePath;

    [ObservableProperty]
    private bool _isSemanticSearchEnabled = App.IsSemanticSearchEnabled;

    partial void OnIsSemanticSearchEnabledChanged(bool value)
    {
        App.IsSemanticSearchEnabled = value;
    }

    [ObservableProperty]
    private Wpf.Ui.Appearance.ApplicationTheme _selectedTheme;

    public SettingsViewModel(IDataService dataService)
    {
        _dataService = dataService;
        _dataFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
            "LinksAndMore", "links.json");
        
        _selectedTheme = ApplicationThemeManager.GetAppTheme();
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

}
