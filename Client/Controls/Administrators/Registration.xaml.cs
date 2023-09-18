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
using Serilog;

namespace Client.Controls.Administrators;

/// <summary>
/// Логика взаимодействия для Registration.xaml
/// </summary>
public partial class Registration : UserControl
{    
    public ILogger _logger { get { return Log.ForContext<Registration>(); } } //логгер для записи логов
    readonly JsonSerializerOptions _settings = new(); //настройки десериализации json
    private readonly List<string> _allowedExtensions = new() { "pdf", "png", "jpeg", "jpg", "bmp" }; //доступные расширения
    private readonly List<char> _allowedSymbolsPhone = new() { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' }; //доступные символы телефона
    string _filePath = string.Empty; //путь к файлу, выбранному пользователем

    /// <summary>
    /// Конструктор контрола регистрации
    /// </summary>
    public Registration()
    {
        try
        {
            /*Инициализация всех компонентов*/
            InitializeComponent();

            /*Выставляем параметры десериализации*/
            _settings.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

            /*Проверяем доступность api*/
            CheckConnection();
        }
        catch (Exception ex)
        {
            _logger.Error("Registration. " + ex.Message);
        }
    }

    /// <summary>
    /// Обработка сохранения по enter
    /// </summary>
    public void Enter(object sender, KeyEventArgs e)
    {
        try
        {
            /*Если нажата кнопка enter*/
            if (e.Key == Key.Enter)
                /*Вызываем метод сохранения*/
                ButtonSave_Click(sender, e);
        }
        catch(Exception ex)
        {
            _logger.Error("Registration. Enter. " + ex.Message);
        }
    }

    /// <summary>
    /// Метод получения ролей
    /// </summary>
    public async void GetRoles()
    {
        try
        {
            /*Объявляем переменную ссылки запроса*/
            string path = null;

            /*Если в конфиге есть данные для формирования ссылки запроса*/
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["DefaultConnection"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Api"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Roles"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Token"]))
            {
                /*Формируем ссылку запроса*/
                path = ConfigurationManager.AppSettings["DefaultConnection"] + ConfigurationManager.AppSettings["Api"] + 
                    ConfigurationManager.AppSettings["Roles"] + "list";

                /*Формируем клиента и добавляем токен*/
                using HttpClient client = new();

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ConfigurationManager.AppSettings["Token"]);

                /*Получаем данные по запросу*/
                using var result = await client.GetAsync(path);

                /*Если получили успешный результат*/
                if (result != null && result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    /*Десериализуем ответ и заполняем combobox ролей*/
                    var content = await result.Content.ReadAsStringAsync();

                    BaseResponseList response = JsonSerializer.Deserialize<BaseResponseList>(content, _settings);

                    Roles.ItemsSource = response.Items;
                }
                else
                {
                    if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        SetError("Некорректный токен", false);
                    else
                        SetError("Ошибка сервера", true);
                }
            }
            else
                SetError("Не указаны адреса api. Обратитесь в техническую поддержку", true);
        }
        catch(Exception ex)
        {
            var serverExceptionStyle = FindResource("CriticalExceptionTextBlock") as Style;
            ErrorText.Style = serverExceptionStyle;
            ErrorText.Text = ex.Message;
        }
    }

    /// <summary>
    /// Метод для перетягивания фотографий
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Image_Drop(object sender, DragEventArgs e)
    {
        try
        {
            /*Если есть фотографий*/
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                /*Получаем массив перетянутых фотографий*/
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop); 
                
                string extention = files[0].Substring(files[0].LastIndexOf('.')+1);

                if (!_allowedExtensions.Contains(extention))
                    SetError("Некорректное расширение файла", false);

                /*Включаем компоненту изображений и отображаем загруженную фото в нём*/
                ImageLoad.IsEnabled = true;
                ImageLoad.Source = new BitmapImage(new Uri(files[0]));

                /*Сохраняем путь к файлу*/
                _filePath = files[0];
            }
        }
        catch(Exception ex)
        {
            _logger.Error("Registration. Image_Drop. " + ex.Message);
        }
    }

    /// <summary>
    /// Метод нажатия кнопки загрузки фотографий
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ButtonLoadImage_Click(object sender, RoutedEventArgs e)
    {
        try
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
                _filePath = openFileDialog.FileName;
            }
        }
        catch(Exception ex)
        {
            _logger.Error("Registration. ButtonLoadImage_Click. " + ex.Message);
        }
    }

    /// <summary>
    /// Метод нажатия на кнопку сохранения данных по регистрации пользователей
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void ButtonSave_Click(object sender, RoutedEventArgs e)
    {
        try        
        {
            /*Отключаем кнопку для нажатия*/
            ButtonSave.IsEnabled = false;

            /*Убираем тест ошибки*/
            ErrorText.Text = null;

            /*Проверяем ошибки*/
            if (String.IsNullOrEmpty(Username.Text) || Username.Text == "Логин")
            {
                SetError("Не указан логин", false);
                return;
            }

            if (String.IsNullOrEmpty(Password.Text) || Password.Text == "Пароль")
            {
                SetError("Не указан пароль", false);
                return;
            }

            if (String.IsNullOrEmpty(Email.Text) || Email.Text == "Почта")
            {
                SetError("Не указана почта", false);
                return;
            }

            if (String.IsNullOrEmpty(PhoneNumber.Text) || PhoneNumber.Text == "Телефон")
            {
                SetError("Не указан телефон", false);
                return;
            }

            if (String.IsNullOrEmpty(LastName.Text) || LastName.Text == "Фамилия")
            {
                SetError("Не указана фамилия", false);
                return;
            }

            if (String.IsNullOrEmpty(FirstName.Text) || FirstName.Text == "Имя")
            {
                SetError("Не указано имя", false);
                return;
            }

            if (String.IsNullOrEmpty(Patronymic.Text) || Patronymic.Text == "Отчество")
            {
                SetError("Не указано отчество", false);
                return;
            }

            if (String.IsNullOrEmpty(_filePath))
            {
                SetError("Не указан файл", false);
                return;
            }

            if (String.IsNullOrEmpty(Roles?.SelectedValue?.ToString()))
            {
                SetError("Не указана роль", false);
                return;
            }

            /*Формируем модель для регистрации пользователей*/
            List<string> roles = new()
            {
                Roles.SelectedValue.ToString()
            };

            AddUserRequest request = new(Username.Text, Password.Text, Email.Text, PhoneNumber.Text, LastName.Text, FirstName.Text, Patronymic.Text, roles);

            /*Если в конфиге есть данные для формирования ссылки запроса*/
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["DefaultConnection"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Api"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Registration"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Token"]))
            {
                /*Формируем ссылку запроса*/
                string url = ConfigurationManager.AppSettings["DefaultConnection"] + ConfigurationManager.AppSettings["Api"] +
                    ConfigurationManager.AppSettings["Registration"] + "add";

                /*Формируем клиента, добавляем ему токен и тело запроса*/
                using HttpClient client = new();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ConfigurationManager.AppSettings["Token"]);
                StringContent stringCintent = new(JsonSerializer.Serialize(request, _settings).ToString(), Encoding.UTF8, "application/json");

                /*Получаем результат запроса*/
                using var result = await client.PostAsync(url, stringCintent);

                /*Если получили успешный результат*/
                if (result != null)
                {
                    if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        SetError("Некорректный токен", false);

                    /*Десериализуем ответ*/
                    var content = await result.Content.ReadAsStringAsync();

                    BaseResponse response = JsonSerializer.Deserialize<BaseResponse>(content, _settings);

                    /*Если успешно, сохраняем фото*/
                    if (response.Success && response.Id != null)
                    {
                        /*Формируем тело запроса*/
                        using var multipartFormContent = new MultipartFormDataContent();

                        /*Загружаем отправляемый файл*/
                        var fileStreamContent = new StreamContent(File.OpenRead(_filePath));

                        /*Получем тип контента*/
                        var extention = _filePath.Substring(_filePath.LastIndexOf('.') + 1);
                        var contentType = "image/" + extention;
                        var fileName = _filePath.Substring(_filePath.LastIndexOf('\\') + 1);

                        /*Устанавливаем заголовок Content-Type*/
                        fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

                        /*Добавляем загруженный файл в MultipartFormDataContent*/
                        multipartFormContent.Add(fileStreamContent, name: "file", fileName: fileName);

                        /*Если в конфиге есть данные для формирования ссылки запроса*/
                        if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["Files"]))
                        {
                            /*Формируем ссылку запроса*/
                            url = ConfigurationManager.AppSettings["DefaultConnection"] + ConfigurationManager.AppSettings["Api"] +
                                ConfigurationManager.AppSettings["Files"] + "add/User/" + response.Id;

                            /*Формируем клиента и добавляем токен доступа*/
                            using HttpClient clientFile = new();
                            clientFile.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ConfigurationManager.AppSettings["Token"]);

                            /*Получаем реузльтат запроса*/
                            using var resultFile = await clientFile.PostAsync(url, multipartFormContent);

                            /*Если получили успешный результат*/
                            if (resultFile != null)
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

                                    Message message = new("Успешно");

                                    message.Show();
                                }
                                else
                                    SetError(response?.Error?.Message ?? "Ошибка сервера", false);
                            }
                            else
                                SetError("Ошибка сервера", true);
                        }
                        /*Иначе возвращаем ошибку*/
                        else
                            SetError("Не указаны адреса api. Обратитесь в техническую поддержку", true);
                    }
                    else
                        SetError(response?.Error?.Message ?? "Ошибка сервера", false);
                }
                else
                    SetError("Ошибка сервера", true);
            }
            /*Иначе возвращаем ошибку*/
            else
                SetError("Не указаны адреса api. Обратитесь в техническую поддержку", true);

            /*Включаем кнопку для нажатия*/
            ButtonSave.IsEnabled = true;

            //MessageBox.Show("Пользователь успешно зергистрирован");
        }
        catch(Exception ex)
        {
            SetError("Не удалось зарегистрировать полоьзователя. Обратитесь в техническую поддержку", true);
            _logger.Error("Registration. ButtonSave_Click. " + ex.Message);
        }
    }

    /// <summary>
    /// Метод обнуления полей ввода по нажатию
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        try
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
        catch (Exception ex)
        {
            _logger.Error("Registration. TextBox_GotFocus. " + ex.Message);
        }
    }

    /// <summary>
    /// Метод возвращения значений по умолчанию полей ввода по потере фокуса
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        try
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
        catch (Exception ex)
        {
            _logger.Error("Registration. TextBox_LostFocus. " + ex.Message);
        }
    }

    /// <summary>
    /// Метод проверки соединения с api
    /// </summary>
    public async void CheckConnection()
    {
        try
        {
            /*Блокируем все элементы*/
            Username.IsEnabled = false;
            Password.IsEnabled = false;
            Email.IsEnabled = false;
            PhoneNumber.IsEnabled = false;
            LastName.IsEnabled = false;
            FirstName.IsEnabled = false;
            Patronymic.IsEnabled = false;
            Roles.IsEnabled = false;
            ButtonSave.IsEnabled = false;
            ButtonLoadImage.IsEnabled = false;

            /*Объявляем переменную ссылки запроса*/
            string url = null;

            try
            {
                /*Если в конфиге есть данные для формирования ссылки запроса*/
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["DefaultConnection"]))
                {
                    /*Формируем ссылку запроса*/
                    url = ConfigurationManager.AppSettings["DefaultConnection"];

                    /*Получаем данные по запросу*/
                    using HttpClient client = new();

                    using var result = await client.GetAsync(url);

                    /*Если получили успешный результат*/
                    if (result != null && result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        /*Разблокируем все элементы*/
                        Username.IsEnabled = true;
                        Password.IsEnabled = true;
                        Email.IsEnabled = true;
                        PhoneNumber.IsEnabled = true;
                        LastName.IsEnabled = true;
                        FirstName.IsEnabled = true;
                        Patronymic.IsEnabled = true;
                        Roles.IsEnabled = true;
                        ButtonSave.IsEnabled = true;
                        ButtonLoadImage.IsEnabled = true;

                        /*Заполняем роли*/
                        GetRoles();
                    }
                    /*Иначе возвращаем ошибку*/
                    else
                        SetError("Сервер временно недоступен, попробуйте позднее или обратитесь в техническую поддержку", true);
                }
                /*Иначе возвращаем ошибку*/
                else
                    SetError("Не указан адрес api. Обратитесь в техническую поддержку", true);
            }
            catch
            {
                SetError("Сервер временно недоступен, попробуйте позднее или обратитесь в техническую поддержку", true);
            }
        }
        catch(Exception ex)
        {
            SetError(ex.Message, true);
        }
    }

    /// <summary>
    /// Метод отображения ошибок
    /// </summary>
    /// <param name="message"></param>
    /// <param name="serverExceprion"></param>
    public void SetError(string message, bool criticalException)
    {
        try
        {
            /*Объявляем переменные*/
            string style; //стиль

            /*Определяем наименование стиля*/
            if (criticalException)
                style = "CriticalExceptionTextBlock";
            else
                style = "InnerExceptionTextBlock";

            /*Находим стиль*/
            var serverExceptionStyle = FindResource(style) as Style;

            /*Устанавливаем текст и стиль*/
            ErrorText.Style = serverExceptionStyle;
            ErrorText.Text = message;

            /*Включаем кнопку сохранения*/
            ButtonSave.IsEnabled = true;
        }
        catch(Exception ex)
        {
            _logger.Error("Registration. SetError. " + ex.Message);
        }
    }

    /// <summary>
    /// Метод изменения номера телефона
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void PhoneNumber_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            if (!String.IsNullOrEmpty(PhoneNumber.Text) && PhoneNumber.Text != "Телефон")
            {
                string phoneNumber = string.Empty;

                /*Проверяем на валидность введённые цифры*/
                if(PhoneNumber.Text.Length > 4 && !_allowedSymbolsPhone.Contains(PhoneNumber.Text[4]))
                {
                    PhoneNumber.Text = PhoneNumber.Text.Substring(0, 4);
                }
                if (PhoneNumber.Text.Length > 5 && !_allowedSymbolsPhone.Contains(PhoneNumber.Text[5]))
                {
                    PhoneNumber.Text = PhoneNumber.Text.Substring(0, 5);
                }
                if (PhoneNumber.Text.Length > 6 && !_allowedSymbolsPhone.Contains(PhoneNumber.Text[6]))
                {
                    PhoneNumber.Text = PhoneNumber.Text.Substring(0, 6);
                }
                if (PhoneNumber.Text.Length > 9 && !_allowedSymbolsPhone.Contains(PhoneNumber.Text[9]))
                {
                    PhoneNumber.Text = PhoneNumber.Text.Substring(0, 9);
                }
                if (PhoneNumber.Text.Length > 10 && !_allowedSymbolsPhone.Contains(PhoneNumber.Text[10]))
                {
                    PhoneNumber.Text = PhoneNumber.Text.Substring(0, 10);
                }
                if (PhoneNumber.Text.Length > 11 && !_allowedSymbolsPhone.Contains(PhoneNumber.Text[11]))
                {
                    PhoneNumber.Text = PhoneNumber.Text.Substring(0, 11);
                }
                if (PhoneNumber.Text.Length > 13 && !_allowedSymbolsPhone.Contains(PhoneNumber.Text[13]))
                {
                    PhoneNumber.Text = PhoneNumber.Text.Substring(0, 13);
                }
                if (PhoneNumber.Text.Length > 14 && !_allowedSymbolsPhone.Contains(PhoneNumber.Text[14]))
                {
                    PhoneNumber.Text = PhoneNumber.Text.Substring(0, 14);
                }
                if (PhoneNumber.Text.Length > 16 && !_allowedSymbolsPhone.Contains(PhoneNumber.Text[16]))
                {
                    PhoneNumber.Text = PhoneNumber.Text.Substring(0, 14);
                }
                if (PhoneNumber.Text.Length > 16 && !_allowedSymbolsPhone.Contains(PhoneNumber.Text[16]))
                {
                    PhoneNumber.Text = PhoneNumber.Text.Substring(0, 16);
                }

                /*Если первая цифра 7 или 8 меняем на +7*/
                if (PhoneNumber.Text[0] == '8' || PhoneNumber.Text[0] == '7')
                {
                    phoneNumber = "+7" + PhoneNumber.Text.Substring(1);
                }
                /*Иначе просто добавляем +7*/
                else
                {
                    /*Если уже не добавили*/
                    if(!PhoneNumber.Text.Contains("+7"))
                        phoneNumber = "+7" + PhoneNumber.Text.Substring(0);
                }

                /*Если есть цирфа после +7, добавляем скобки*/
                if (PhoneNumber.Text.Length > 2 && PhoneNumber.Text[2] != ' ')
                {
                    phoneNumber = PhoneNumber.Text.Substring(0, 2) + " (" + PhoneNumber.Text.Substring(2);
                }
                if (phoneNumber.Length > 2 && phoneNumber[2] != ' ')
                {
                    phoneNumber = phoneNumber.Substring(0, 2) + " (" + phoneNumber.Substring(2);
                }

                /*Если это последняя цирфа кода оператора, добавляем скобки*/
                if (PhoneNumber.Text.Length > 6 && PhoneNumber.Text[6] != ' ')
                {
                    /*Если уже не добавили*/
                    if (PhoneNumber.Text.Length < 8 || (PhoneNumber.Text.Length > 7 && PhoneNumber.Text[7] != ')'))
                        phoneNumber = PhoneNumber.Text.Substring(0, 7) + ") " + PhoneNumber.Text.Substring(7);
                }

                /*Если это последняя цифра первых 3 цифр после кода оператора, добавляем тире*/
                if (PhoneNumber.Text.Length > 11 && PhoneNumber.Text[11] != ' ')
                {
                    /*Если уже не добавили*/
                    if (PhoneNumber.Text.Length < 13 || (PhoneNumber.Text.Length > 12 && PhoneNumber.Text[12] != '-'))
                        phoneNumber = PhoneNumber.Text.Substring(0, 12) + "-" + PhoneNumber.Text.Substring(12);
                }

                /*Если это последняя цифра первых 5 цифр после кода оператора, добавляем тире*/
                if (PhoneNumber.Text.Length > 14 && PhoneNumber.Text[14] != ' ')
                {
                    /*Если уже не добавили*/
                    if (PhoneNumber.Text.Length < 16 || (PhoneNumber.Text.Length > 15 && PhoneNumber.Text[15] != '-'))
                        phoneNumber = PhoneNumber.Text.Substring(0, 15) + "-" + PhoneNumber.Text.Substring(15);
                }

                if (!String.IsNullOrEmpty(phoneNumber))
                    PhoneNumber.Text = phoneNumber;

                PhoneNumber.CaretIndex = PhoneNumber.Text.Length;
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Registration. PhoneNumber_TextChanged. " + ex.Message);
        }
    }
}