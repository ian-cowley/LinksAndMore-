using System.Windows.Controls;
using LinksAndMore.ViewModels;
using LinksAndMore.Services;

namespace LinksAndMore.Views;

public partial class SettingsPage : Page
{
    public SettingsPage()
    {
        InitializeComponent();
        DataContext = new SettingsViewModel(App.DataService);
    }
}
