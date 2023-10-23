using Domain.Models.Identification.Authorization.Response;
using Client.Services.Base;
using Microsoft.AspNetCore.WebUtilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Queries;

namespace Client.Controls;

/// <summary>
/// Логика взаимодействия для Authoriztion.xaml
/// </summary>
public partial class Authorization : UserControl
{
    public ILogger _logger { get { return Log.ForContext<Authorization>(); } } //логгер для записи логов
    public IBaseService _baseService; //базовый сервис
    private ConfigurationFile _configuration; //класс конфигурации

    /// <summary>
    /// Конструктор страницы авторизации
    /// </summary>
    public Authorization(IBaseService baseService)
    {
        try
        {
            //Инициализация компонентов
            InitializeComponent();

            //Получаем базовый сервис
            _baseService = baseService;

            //Создаём экземпляр класса конфигурации
            _configuration = new();
        }
        catch(Exception ex)
        {
            _logger.Error("Authorization. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Обработка авторизации по enter
    /// </summary>
    public void Enter(object sender, KeyEventArgs e)
    {
        try
        {
            if (e.Key == Key.Enter)
                ButtonLogin_Click(sender, e);
        }
        catch (Exception ex)
        {
            _logger.Error("Authorization. Enter. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Событие нажатия на кнопку входа
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void ButtonLogin_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        try
        {
            //Отключаем для нажатия кнопку авторизации
            ButtonLogin.IsEnabled = false;

            //Объявляем переменные
            string username = Username.Text; //логин
            string password = Password.Text; //пароль

            //Проверяем корректность данных
            if (String.IsNullOrEmpty(username) || username == "Логин")
            {
                SetError("Не указан логин", false);
                return;
            }
            if (String.IsNullOrEmpty(password) || password == "Пароль")
            {
                Console.WriteLine("Не указан пароль");
                return;
            }

            //Если в конфиге есть данные для формирования ссылки запроса
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["DefaultConnection"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Api"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Authorization"]))
            {
                //Формируем ссылку запроса
                string url = ConfigurationManager.AppSettings["DefaultConnection"] + ConfigurationManager.AppSettings["Api"] +
                    ConfigurationManager.AppSettings["Authorization"] + "login";

                //Добавляем параметры строки
                var queryParams = new Dictionary<string, string>
                {
                    ["username"] = username,
                    ["password"] = password
                };

                //Получаем данные по запросу
                using HttpClient client = new();

                //Получаем результат запроса
                using var result = await client.GetAsync(QueryHelpers.AddQueryString(url, queryParams));

                //Если получили успешный результат
                if (result != null)
                {
                    //Десериализуем ответ
                    var content = await result.Content.ReadAsStringAsync();
                    AuthorizationResponse response = JsonSerializer
                        .Deserialize<AuthorizationResponse>(content, _baseService.GetJsonSettings());

                    //Если успешно, записываем в конфиг токен, получаем права доступа и открываем основное окно
                    if (response.Success)
                    {
                        ConfigurationManager.AppSettings["Token"] = response.Token;
                        _configuration.SetValue("Token", response.Token);
                        var accessRights = await GetAccessRights();
                        Base main = new(_baseService, accessRights);
                        Content = main;
                    }
                    else
                        SetError(response?.Error?.Message ?? "Ошибка сервера", false);
                }
                else
                    SetError("Ошибка сервера", true);
            }
            //Иначе возвращаем ошибку
            else
                SetError("Не указаны адреса api.Обратитесь в техническую поддержку", true);
        }
        catch(Exception ex)
        {
            _logger.Error("Authorization. ButtonLogin_Click. Ошибка: {0}", ex);
        }
        finally
        {
            ButtonLogin.IsEnabled = true;
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
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Authorization. TextBox_GotFocus. Ошибка: {0}", ex);
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
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Authorization. TextBox_LostFocus. Ошибка: {0}", ex);
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
            //Объявляем переменные
            string style; //стиль

            //Определяем наименование стиля
            if (criticalException)
                style = "CriticalExceptionTextBlock";
            else
                style = "InnerExceptionTextBlock";

            //Находим стиль
            var serverExceptionStyle = FindResource(style) as Style;

            //Устанавливаем текст и стиль
            Error.Style = serverExceptionStyle;
            Error.Text = message;
        }
        catch (Exception ex)
        {
            _logger.Error("Authorization. SetError. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Событие загрузки окна
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            //Отключаем кнопку авторизации
            ButtonLogin.IsEnabled = false;

            //Проверяем соединение
            await _baseService.CheckConnection();
            
            //Включаем кнопку авторизации
            ButtonLogin.IsEnabled = true;
        }
        catch(Exception ex)
        {
            SetError(ex.Message, true);
        }
    }

    /// <summary>
    /// Метод получения прав доступа
    /// </summary>
    /// <returns></returns>
    public async Task<List<string>> GetAccessRights()
    {
        try
        {
            //Получаем информацию о пользователе
            var userInfo = await _baseService.GetUserInfo();

            //Если получили информацию о пользователе
            if (userInfo != null)
            {
                //Возвращаем список прав доступа
                return userInfo.AccessRights;
            }
            //Иначе
            else
            {
                //Отображаем ошибку
                _baseService.SetError("Не удалось получить информацию о пользователе");

                //Возвращаем пустоту
                return null;
            }
        }
        //В случае ошибки
        catch (Exception ex)
        {
            //Логгируем ошибку
            _logger.Error("Authorization. VisibleAdminButton. Ошибка: {0}", ex);

            //Возвращаем пустоту
            return null;
        }
    }
}