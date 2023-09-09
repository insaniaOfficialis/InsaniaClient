using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Client.Controls.Administrators;

/// <summary>
/// Логика взаимодействия для Registration.xaml
/// </summary>
public partial class Registration : UserControl
{
    public Registration()
    {
        InitializeComponent();
    }

    private void Image_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            ImageLoad.IsEnabled = true;
            ImageLoad.Source = new BitmapImage(new Uri(files[0]));
        }
    }

    private void ButtonLoadImage_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new();
        openFileDialog.Filter = "Image files (*.txt)|*.txt|All files (*.*)|*.*";
        /*if (openFileDialog.ShowDialog() == true)
            txtEditor.Text = File.ReadAllText(openFileDialog.FileName);*/
    }
}
