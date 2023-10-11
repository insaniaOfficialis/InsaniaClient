using Client.Models.Identification.Authorization.Response;
using Microsoft.AspNetCore.WebUtilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client.Controls;

/// <summary>
/// Логика взаимодействия для Authoriztion.xaml
/// </summary>
public partial class Authorization : UserControl
{
    public ILogger _logger { get { return Log.ForContext<Authorization>(); } } //логгер для записи логов
    readonly JsonSerializerOptions _jsonSettings = new(); //настройки десериализации json

    /// <summary>
    /// Конструктор страницы авторизации
    /// </summary>
    public Authorization()
    {
        try
        {
            /*Инициализация компонентов*/
            InitializeComponent();

            /*Выставлыяем параметры десериализации*/
            _jsonSettings.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

            /*Проверяем соединения с api*/
            CheckConnection();
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
            /*Отключаем для нажатия кнопку авторизации*/
            ButtonLogin.IsEnabled = false;

            /*Объявляем переменные*/
            string username = Username.Text; //логин
            string password = Password.Text; //пароль

            /*Проверяем корректность данных*/
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

            /*Если в конфиге есть данные для формирования ссылки запроса*/
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["DefaultConnection"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Api"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Authorization"]))
            {
                /*Формируем ссылку запроса*/
                string url = ConfigurationManager.AppSettings["DefaultConnection"] + ConfigurationManager.AppSettings["Api"] +
                    ConfigurationManager.AppSettings["Authorization"] + "login";

                /*Добавляем параметры строки*/
                var queryParams = new Dictionary<string, string>
                {
                    ["username"] = username,
                    ["password"] = password
                };

                /*Получаем данные по запросу*/
                using HttpClient client = new();

                /*Получаем результат запроса*/
                using var result = await client.GetAsync(QueryHelpers.AddQueryString(url, queryParams));

                /*Если получили успешный результат*/
                if (result != null)
                {
                    /*Десериализуем ответ*/
                    var content = await result.Content.ReadAsStringAsync();

                    AuthorizationResponse response = JsonSerializer.Deserialize<AuthorizationResponse>(content, _jsonSettings);

                    /*Если успешно, открываем основное окно и записываем в конфиг токен*/
                    if (response.Success)
                    {
                        ConfigurationManager.AppSettings["Token"] = response.Token;

                        Base main = new();

                        Content = main;
                    }
                    else
                        SetError(response?.Error?.Message ?? "Ошибка сервера", false);
                }
                else
                    SetError("Ошибка сервера", true);
            }
            /*Иначе возвращаем ошибку*/
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
            Error.Style = serverExceptionStyle;
            Error.Text = message;
        }
        catch (Exception ex)
        {
            _logger.Error("Authorization. SetError. Ошибка: {0}", ex);
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
            ButtonLogin.IsEnabled = false;

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
                        ButtonLogin.IsEnabled = true;
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
        catch (Exception ex)
        {
            SetError(ex.Message, true);
        }
    }
}