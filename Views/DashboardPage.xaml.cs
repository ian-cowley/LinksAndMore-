using System.Windows.Controls;
using LinksAndMore.ViewModels;
using LinksAndMore.Services;
using LinksAndMore.Models;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ui = Wpf.Ui.Controls;

namespace LinksAndMore.Views;

public partial class DashboardPage
{
    public DashboardPage()
    {
        InitializeComponent();
        
        DataContext = new DashboardViewModel(App.DataService);
    }

    private async Task<EditItemDialog?> ShowEditItemDialogAsync(DashboardItem item, DashboardViewModel viewModel, Category? currentCategory)
    {
        var allTitles = viewModel.Categories
            .SelectMany(c => c.Items)
            .Select(i => i.Title)
            .Distinct()
            .OrderBy(t => t);

        var dialogContent = new EditItemDialog();
        dialogContent.LoadItem(item, viewModel.Categories, allTitles, currentCategory);

        var mainWindow = (MainWindow)System.Windows.Application.Current.MainWindow;

        var dialog = new Ui.ContentDialog()
        {
            DialogHostEx = mainWindow.RootContentDialogPresenter,
            Content = dialogContent
        };

        Ui.ContentDialogResult customResult = Ui.ContentDialogResult.None;

        dialogContent.SaveClicked += (s, args) =>
        {
            customResult = Ui.ContentDialogResult.Primary;
            dialog.Hide();
        };

        dialogContent.CancelClicked += (s, args) =>
        {
            dialog.Hide();
        };

        await dialog.ShowAsync();

        if (customResult == Ui.ContentDialogResult.Primary)
        {
            dialogContent.SaveToItem(item);
            return dialogContent;
        }

        return null;
    }

    private async void OnAddItemClick(object sender, System.Windows.RoutedEventArgs e)
    {
        var viewModel = (DashboardViewModel)DataContext;
        var newItem = new DashboardItem { Title = "", Type = ItemType.Link };
        
        var resultDialog = await ShowEditItemDialogAsync(newItem, viewModel, viewModel.Categories.FirstOrDefault());

        if (resultDialog != null)
        {
            var targetCategory = resultDialog.SelectedCategory;
            
            if (targetCategory != null)
            {
                targetCategory.Items.Add(newItem);
                await viewModel.SaveDataCommand.ExecuteAsync(null);
                viewModel.Refresh();
            }
        }
    }

    private async void OnEditItemClick(object sender, System.Windows.RoutedEventArgs e)
    {
        if (sender is Ui.Button button && button.CommandParameter is DashboardItem item)
        {
            var viewModel = (DashboardViewModel)DataContext;
            
            // Find current category
            var currentCategory = viewModel.Categories.FirstOrDefault(c => c.Items.Contains(item));

            var resultDialog = await ShowEditItemDialogAsync(item, viewModel, currentCategory);

            if (resultDialog != null)
            {
                var targetCategory = resultDialog.SelectedCategory;

                // Handle category move
                if (targetCategory != null && targetCategory != currentCategory)
                {
                    currentCategory?.Items.Remove(item);
                    targetCategory.Items.Add(item);
                }

                await viewModel.SaveDataCommand.ExecuteAsync(null);
                viewModel.Refresh();
            }
        }
    }
}
