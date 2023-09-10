using Client.Models.Base;
using Client.Models.Identification.Registration.Request;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Input;

namespace Client.Controls.Administrators;

/// <summary>
/// Логика взаимодействия для Registration.xaml
/// </summary>
public partial class Registration : UserControl
{
    JsonSerializerOptions _settings = new(); //настройки десериализации json
    string filePath = string.Empty; //путь к файлу, выбранному пользователем
    
    /// <summary>
    /// Конструктор контрола регистрации
    /// </summary>
    public Registration()
    {
        /*Инициализация всех компонентов*/
        InitializeComponent();

        /*Выставлыяем параметры десириализации*/
        _settings.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

        /*Определяем действие для кнопки enter*/
        PreviewKeyDown += new KeyEventHandler(Enter);

        /*Заполняем роли*/
        GetRoles();
    }

    /// <summary>
    /// Обработка сохранения по enter
    /// </summary>
    public void Enter(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            ButtonSave_Click(sender, e);
    }

    /// <summary>
    /// Метод получения ролей
    /// </summary>
    public async void GetRoles()
    {
        /*Объявляем переменную ссылки запроса*/
        string path = null;

        /*Если в конфиге есть данные для формирования ссылки запроса*/
        if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["DefaultConnection"])
            && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Api"])
            && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Roles"]))
        {
            /*Формируем ссылку запроса*/
            path = ConfigurationManager.AppSettings["DefaultConnection"] + ConfigurationManager.AppSettings["Api"] + ConfigurationManager.AppSettings["Roles"] + "list";
        }

        /*Получаем данные по запросу*/
        using HttpClient client = new();
        var result = await client.GetAsync(path);

        /*Если получили успешный результат*/
        if (result != null && result.StatusCode == System.Net.HttpStatusCode.OK)
        {
            /*Десериализуем ответ и заполняем combobox ролей*/
            var content = await result.Content.ReadAsStringAsync();
            
            BaseResponseList response = JsonSerializer.Deserialize<BaseResponseList>(content, _settings);

            Roles.ItemsSource = response.Items;
        }
    }

    /// <summary>
    /// Метод для перетягивания фотографий
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Image_Drop(object sender, DragEventArgs e)
    {
        /*Если есть фотографий*/
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            /*Получаем массив перетянутых фотографий*/
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            /*Включаем компоненту изображений и отображаем загруженную фото в нём*/
            ImageLoad.IsEnabled = true;
            ImageLoad.Source = new BitmapImage(new Uri(files[0]));

            /*Сохраняем путь к файлу*/
            filePath = files[0];
        }
    }

    /// <summary>
    /// Метод нажатия кнопки загрузки фотографий
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ButtonLoadImage_Click(object sender, RoutedEventArgs e)
    {
        /*Формируем новое окно загрузки фотографий*/
        OpenFileDialog openFileDialog = new();

        /*Ставим фильтры на типы файлов*/
        openFileDialog.Filter = "Image files (*.pdf;*.png;*.jpeg;*.jpg;*.bmp)|*.pdf;*.png;*.jpeg;*.jpg;*.bmp";

        /*Если есть выбранные файлы,*/
        if (openFileDialog.ShowDialog() == true)
        {
            /*Включаем компоненту изображений и отображаем выбранное фото в нём*/
            ImageLoad.IsEnabled = true;
            ImageLoad.Source = new BitmapImage(new Uri(openFileDialog.FileName));

            /*Сохраняем путь к файлу*/
            filePath = openFileDialog.FileName;
        }
    }

    /// <summary>
    /// Метод нажатия на кнопку сохранения данных по регистрации пользователей
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void ButtonSave_Click(object sender, RoutedEventArgs e)
    {
        /*Отключаем кнопку для нажатия*/
        ButtonSave.IsEnabled = false;

        /*Формируем модель для регистрации пользователей*/
        List<string> roles = new()
        {
            Roles.SelectedValue.ToString()
        };

        AddUserRequest request = new(Username.Text, Password.Text, Email.Text, PhoneNumber.Text, LastName.Text, FirstName.Text, Patronymic.Text, roles);

        /*Объявляем переменную ссылки запроса*/
        string url = null;

        /*Если в конфиге есть данные для формирования ссылки запроса*/
        if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["DefaultConnection"])
            && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Api"])
            && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Registration"]))
        {
            /*Формируем ссылку запроса*/
            url = ConfigurationManager.AppSettings["DefaultConnection"] + ConfigurationManager.AppSettings["Api"] +
                ConfigurationManager.AppSettings["Registration"] + "add";
        }

        /*Получаем данные по запросу*/
        using HttpClient client = new();

#if DEBUG
        var a = JsonSerializer.Serialize(request, _settings).ToString();
#endif
        StringContent stringCintent = new(JsonSerializer.Serialize(request, _settings).ToString(), Encoding.UTF8, "application/json");
        using var result = await client.PostAsync(url, stringCintent);

        /*Если получили успешный результат*/
        if (result != null && result.StatusCode == System.Net.HttpStatusCode.OK)
        {
            /*Десериализуем ответ*/
            var content = await result.Content.ReadAsStringAsync();

            BaseResponse response = JsonSerializer.Deserialize<BaseResponse>(content, _settings);

            /*Если успешно, сохраняем фото*/
            if(response.Success && response.Id != null)
            {
                /*Формируем тело запроса*/
                using var multipartFormContent = new MultipartFormDataContent();

                /*Загружаем отправляемый файл*/
                var fileStreamContent = new StreamContent(File.OpenRead(filePath));

                /*Получем тип контента*/
                var extention = filePath.Substring(filePath.LastIndexOf('.') + 1);
                var contentType = "image/" + extention;
                var fileName = filePath.Substring(filePath.LastIndexOf('\\') + 1);

                /*Устанавливаем заголовок Content-Type*/
                fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

                /*Добавляем загруженный файл в MultipartFormDataContent*/
                multipartFormContent.Add(fileStreamContent, name: "file", fileName: fileName);

                /*Если в конфиге есть данные для формирования ссылки запроса*/
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["DefaultConnection"])
                    && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Api"])
                    && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Files"]))
                {
                    /*Формируем ссылку запроса*/
                    url = ConfigurationManager.AppSettings["DefaultConnection"] + ConfigurationManager.AppSettings["Api"] +
                        ConfigurationManager.AppSettings["Files"] + "add/User/" + response.Id;
                }

                /*Отправляем файл*/
                using HttpClient clientFile = new();
                using var resultFile = await clientFile.PostAsync(url, multipartFormContent);

#if DEBUG
                var b = await resultFile.Content.ReadAsStringAsync();
#endif

                /*Если получили успешный результат*/
                if (resultFile != null && resultFile.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    /*Десериализуем ответ*/
                    var contentFile = await resultFile.Content.ReadAsStringAsync();

                    BaseResponse responseFile = JsonSerializer.Deserialize<BaseResponse>(contentFile, _settings);

                    /*Если успешно, обнуляем поля*/
                    if (responseFile.Success)
                    {
                        Username.Text = "Логин";
                        Password.Text = "Пароль";
                        Email.Text = "Почта";
                        PhoneNumber.Text = "Телефон";
                        LastName.Text = "Фамилия";
                        FirstName.Text = "Имя";
                        Patronymic.Text = "Отчество";
                        Roles.SelectedItem = null;
                        Roles.Text = "Роли";
                        ImageLoad.Source = null;
                        ImageLoad.IsEnabled = false;
                    }
                }
            }
        }

        /*Включаем кнопку для нажатия*/
        ButtonSave.IsEnabled = true;
    }

    /// <summary>
    /// Метод обнуления полей ввода по нажатию
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        var textbox = sender as TextBox;

        switch (textbox.Name)
        {
            case "Username":
                {
                    if (Username.Text == "Логин")
                        Username.Text = "";

                }
                break;
            case "Password":
                {
                    if (Password.Text == "Пароль")
                        Password.Text = "";

                }
                break;
            case "Email":
                {
                    if (Email.Text == "Почта")
                        Email.Text = "";

                }
                break;
            case "PhoneNumber":
                {
                    if (PhoneNumber.Text == "Телефон")
                        PhoneNumber.Text = "";

                }
                break;
            case "LastName":
                {
                    if (LastName.Text == "Фамилия")
                        LastName.Text = "";

                }
                break;
            case "FirstName":
                {
                    if (FirstName.Text == "Имя")
                        FirstName.Text = "";

                }
                break;
            case "Patronymic":
                {
                    if (Patronymic.Text == "Отчество")
                        Patronymic.Text = "";

                }
                break;
        }
    }

    /// <summary>
    /// Метод возвращения значений по умолчанию полей ввода по потере фокуса
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        var textbox = sender as TextBox;

        switch (textbox.Name)
        {
            case "Username":
                {
                    if (Username.Text == "")
                        Username.Text = "Логин";

                }
                break;
            case "Password":
                {
                    if (Password.Text == "")
                        Password.Text = "Пароль";

                }
                break;
            case "Email":
                {
                    if (Email.Text == "")
                        Email.Text = "Почта";

                }
                break;
            case "PhoneNumber":
                {
                    if (PhoneNumber.Text == "")
                        PhoneNumber.Text = "Телефон";

                }
                break;
            case "LastName":
                {
                    if (LastName.Text == "")
                        LastName.Text = "Фамилия";

                }
                break;
            case "FirstName":
                {
                    if (FirstName.Text == "")
                        FirstName.Text = "Имя";

                }
                break;
            case "Patronymic":
                {
                    if (Patronymic.Text == "")
                        Patronymic.Text = "Отчество";

                }
                break;
        }
    }
}
