using System.Windows.Controls;
using LinksAndMore.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LinksAndMore.Views;

public partial class EditItemDialog : UserControl
{
    public EditItemDialog()
    {
        InitializeComponent();
    }

    public void LoadItem(DashboardItem item, ObservableCollection<Category> categories, IEnumerable<string> titleSuggestions, Category? selectedCategory = null)
    {
        TitleComboBox.ItemsSource = titleSuggestions;
        TitleComboBox.Text = item.Title;
        
        ContentTextBox.Text = item.Content;
        DescriptionTextBox.Text = item.Description;
        
        CategoryComboBox.ItemsSource = categories;
        CategoryComboBox.SelectedItem = selectedCategory;

        foreach (ComboBoxItem cbItem in TypeComboBox.Items)
        {
            if (cbItem.Tag is ItemType type && type == item.Type)
            {
                TypeComboBox.SelectedItem = cbItem;
                break;
            }
        }
    }

    public void SaveToItem(DashboardItem item)
    {
        item.Title = TitleComboBox.Text;
        item.Content = ContentTextBox.Text;
        item.Description = DescriptionTextBox.Text;
        if (TypeComboBox.SelectedItem is ComboBoxItem cbItem && cbItem.Tag is ItemType type)
        {
            item.Type = type;
        }
    }

    public Category? SelectedCategory => CategoryComboBox.SelectedItem as Category;
}
