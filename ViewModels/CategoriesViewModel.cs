using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LinksAndMore.Models;
using LinksAndMore.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace LinksAndMore.ViewModels;

public partial class CategoriesViewModel : ObservableObject
{
    private readonly IDataService _dataService;

    [ObservableProperty]
    private ObservableCollection<Category> _categories;

    [ObservableProperty]
    private string _newCategoryName = string.Empty;

    public CategoriesViewModel(IDataService dataService)
    {
        _dataService = dataService;
        _categories = new ObservableCollection<Category>();
        _ = LoadData();
    }

    public async Task LoadData()
    {
        try
        {
            Categories.Clear();
            var loadedCategories = await _dataService.LoadDataAsync();
            foreach (var category in loadedCategories)
            {
                Categories.Add(category);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading categories data: {ex.Message}");
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
