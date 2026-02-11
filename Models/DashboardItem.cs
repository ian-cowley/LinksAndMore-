using CommunityToolkit.Mvvm.ComponentModel;

namespace LinksAndMore.Models;

public enum ItemType
{
    Link,
    Note,
    Snippet
}

public partial class DashboardItem : ObservableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _content = string.Empty; // URL for Link, Text for Note/Snippet

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private ItemType _type = ItemType.Link;
}
