using System.Windows.Controls;
using LinksAndMore.ViewModels;

namespace LinksAndMore.Views;

public partial class CategoriesPage : Page
{
    public CategoriesPage()
    {
        InitializeComponent();
        DataContext = new CategoriesViewModel(App.DataService);
        
        Loaded += (s, e) => 
        {
            if (DataContext is CategoriesViewModel vm)
            {
                _ = vm.LoadData();
            }
        };
    }
}
