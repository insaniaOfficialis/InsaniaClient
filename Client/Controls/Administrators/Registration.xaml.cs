using Client.Models.Base;
using Microsoft.Win32;
using System;
using System.Configuration;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Client.Controls.Administrators;

/// <summary>
/// Логика взаимодействия для Registration.xaml
/// </summary>
public partial class Registration : UserControl
{
    JsonSerializerOptions _settings = new();
    public Registration()
    {
        InitializeComponent();

        _settings.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

        GetRoles();
    }

    public async void GetRoles()
    {
        string path = null;

        if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["DefaultConnection"])
            && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Api"])
            && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Roles"]))
        {
            path = ConfigurationManager.AppSettings["DefaultConnection"] + ConfigurationManager.AppSettings["Api"] + ConfigurationManager.AppSettings["Roles"] + "list";
        }

        using HttpClient client = new();
        var result = await client.GetAsync(path);

        if (result != null && result.StatusCode == System.Net.HttpStatusCode.OK)
        {
            var content = await result.Content.ReadAsStringAsync();
            BaseResponseList response = JsonSerializer.Deserialize<BaseResponseList>(content, _settings);

            Roles.ItemsSource = response.Items;
        }
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
        openFileDialog.Filter = "Image files (*.pdf;*.png;*.jpeg;*.jpg;*.bmp)|*.pdf;*.png;*.jpeg;*.jpg;*.bmp";
        if (openFileDialog.ShowDialog() == true)
            ImageLoad.Source = new BitmapImage(new Uri(openFileDialog.FileName));
    }
}
