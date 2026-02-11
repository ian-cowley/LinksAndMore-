using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LinksAndMore.Models;

public partial class Category : ObservableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string? _icon; // Wpf.Ui.Common.SymbolRegular as string

    public ObservableCollection<DashboardItem> Items { get; set; } = new();

    [ObservableProperty]
    private ObservableCollection<DashboardItem> _filteredItems = new();
}
