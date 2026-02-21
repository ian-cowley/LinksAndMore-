using System.IO;
using System.Text.Json;
using System.Collections.ObjectModel;
using LinksAndMore.Models;
using LinksAndMore.Data;
using Microsoft.EntityFrameworkCore;

namespace LinksAndMore.Services;

public interface IDataService
{
    Task<ObservableCollection<Category>> LoadDataAsync();
    Task SaveDataAsync(ObservableCollection<Category> categories);
}

public class DataService : IDataService
{
    private static readonly string DataDirPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
        "LinksAndMore");
    private static readonly string JsonDataFilePath = Path.Combine(DataDirPath, "links.json");
    private static readonly string DbFilePath = Path.Combine(DataDirPath, "links.db");

    private readonly DbContextOptions<AppDbContext> _dbOptions;

    public DataService()
    {
        if (!Directory.Exists(DataDirPath))
        {
            Directory.CreateDirectory(DataDirPath);
        }

        var builder = new DbContextOptionsBuilder<AppDbContext>();
        builder.UseSqlite($"Data Source={DbFilePath}");
        _dbOptions = builder.Options;

        // Ensure database is created and run migration from JSON
        using var db = new AppDbContext(_dbOptions);
        db.Database.EnsureCreated();
        MigrateJsonDataIfNeeded();
    }

    private void MigrateJsonDataIfNeeded()
    {
        if (File.Exists(JsonDataFilePath))
        {
            try
            {
                using var db = new AppDbContext(_dbOptions);
                if (!db.Categories.Any()) // Only migrate if DB is empty
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    string json = File.ReadAllText(JsonDataFilePath);
                    var categories = JsonSerializer.Deserialize<List<Category>>(json, options);
                    
                    if (categories != null)
                    {
                        foreach (var cat in categories)
                        {
                            db.Categories.Add(cat);
                        }
                        db.SaveChanges();
                    }
                }
                
                // Backup the JSON file so we don't migrate again
                File.Move(JsonDataFilePath, JsonDataFilePath + ".bak");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Migration failed: {ex.Message}");
            }
        }
    }

    public async Task<ObservableCollection<Category>> LoadDataAsync()
    {
        using var db = new AppDbContext(_dbOptions);
        var categories = await db.Categories
            .Include(c => c.Items)
            .ToListAsync();
            
        return new ObservableCollection<Category>(categories);
    }

    public async Task SaveDataAsync(ObservableCollection<Category> categories)
    {
        using var db = new AppDbContext(_dbOptions);
        
        var existingCategories = await db.Categories.Include(c => c.Items).ToListAsync();
        
        var categoryIdsToKeep = categories.Select(c => c.Id).ToList();
        var categoriesToRemove = existingCategories.Where(c => !categoryIdsToKeep.Contains(c.Id)).ToList();
        
        db.Categories.RemoveRange(categoriesToRemove);
        
        foreach (var category in categories)
        {
            var existingCategory = existingCategories.FirstOrDefault(c => c.Id == category.Id);
            if (existingCategory == null)
            {
                db.Categories.Add(category);
            }
            else
            {
                db.Entry(existingCategory).CurrentValues.SetValues(category);
                
                var itemIdsToKeep = category.Items.Select(i => i.Id).ToList();
                var itemsToRemove = existingCategory.Items.Where(i => !itemIdsToKeep.Contains(i.Id)).ToList();
                db.Items.RemoveRange(itemsToRemove);
                
                foreach (var item in category.Items)
                {
                    var existingItem = existingCategory.Items.FirstOrDefault(i => i.Id == item.Id);
                    if (existingItem == null)
                    {
                        existingCategory.Items.Add(item);
                    }
                    else
                    {
                        db.Entry(existingItem).CurrentValues.SetValues(item);
                    }
                }
            }
        }
        
        await db.SaveChangesAsync();
    }
}
