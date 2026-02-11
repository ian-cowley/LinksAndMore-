using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace LinksAndMore.Models;

public enum ItemType
{
    Link,
    Note,
    Snippet,
    Password
}

public partial class DashboardItem : ObservableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _content = string.Empty; // URL for Link, Text for Note/Snippet/Password

    [ObservableProperty]
    [property: JsonIgnore]
    private bool _isLocked = true;

    [ObservableProperty]
    [property: JsonIgnore]
    private string _decryptedContent = string.Empty;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private ItemType _type = ItemType.Link;
}
