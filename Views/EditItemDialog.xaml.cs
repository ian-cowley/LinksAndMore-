using System.Windows.Controls;
using LinksAndMore.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;
using System.Windows;

namespace LinksAndMore.Views;

public partial class EditItemDialog : UserControl
{
    public event EventHandler? SaveClicked;
    public event EventHandler? CancelClicked;

    public EditItemDialog()
    {
        InitializeComponent();
    }

    public void LoadItem(DashboardItem item, ObservableCollection<Category> categories, IEnumerable<string> titleSuggestions, Category? selectedCategory = null)
    {
        TitleComboBox.ItemsSource = titleSuggestions;
        TitleComboBox.Text = item.Title;
        
        // If it's a password, we show the decrypted content if available, otherwise blank
        if (item.Type == ItemType.Password)
        {
            ContentTextBox.Text = string.IsNullOrEmpty(item.DecryptedContent) ? 
                (string.IsNullOrEmpty(item.Content) ? "" : "[Protected]") : 
                item.DecryptedContent;
        }
        else
        {
            ContentTextBox.Text = item.Content;
        }

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
        
        if (TypeComboBox.SelectedItem is ComboBoxItem cbItem && cbItem.Tag is ItemType type)
        {
            item.Type = type;
            
            if (type == ItemType.Password)
            {
                // Only encrypt if the content doesn't look like our placeholder
                if (ContentTextBox.Text != "[Protected]")
                {
                    item.Content = App.SecurityService.Encrypt(ContentTextBox.Text);
                    item.DecryptedContent = ContentTextBox.Text;
                    item.IsLocked = false;
                }
            }
            else
            {
                item.Content = ContentTextBox.Text;
            }
        }
        else
        {
            item.Content = ContentTextBox.Text;
        }

        item.Description = DescriptionTextBox.Text;
    }

    public Category? SelectedCategory => CategoryComboBox.SelectedItem as Category;

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
        SaveClicked?.Invoke(this, EventArgs.Empty);
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        CancelClicked?.Invoke(this, EventArgs.Empty);
    }
}
