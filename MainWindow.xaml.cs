using System.Windows;
using LinksAndMore.Views;

namespace LinksAndMore;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        
        Loaded += (s, e) => 
        {
            RootNavigation.Navigate(typeof(DashboardPage));
        };
    }
}