using Client.Models.Base;
using Client.Models.Identification.Registration.Request;
using Client.Models.Identification.Roles.Request;
using DevExpress.Xpf.Editors.Internal;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace Client.Controls.Administrators;

/// <summary>
/// Логика взаимодействия для Roles.xaml
/// </summary>
public partial class Roles : UserControl
{
    public ILogger _logger { get { return Log.ForContext<Roles>(); } } //сервис для записи логов
    readonly JsonSerializerOptions _settings = new(); //настройки десериализации json

    /// <summary>
    /// Конструктор страницы ролей
    /// </summary>
    public Roles()
    {
        try
        {
            /*Инициализация всех компонентов*/
            InitializeComponent();

            /*Выставлыяем параметры десериализации*/
            _settings.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

            /*Проверяем доступность api*/
            CheckConnection();
        }
        catch (Exception ex)
        {
            _logger.Error("Roles. " + ex.Message);
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
            NameTextBox.IsEnabled = false;
            SaveButton.IsEnabled = false;

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
                        NameTextBox.IsEnabled = true;
                        SaveButton.IsEnabled = true;
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

    /// <summary>
    /// Метод отображения ошибок
    /// </summary>
    /// <param name="message"></param>
    /// <param name="criticalException"></param>
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
            var exceptionStyle = FindResource(style) as Style;

            /*Устанавливаем текст и стиль*/
            ErrorTextBlock.Style = exceptionStyle;
            ErrorTextBlock.Text = message;
        }
        catch (Exception ex)
        {
            _logger.Error("Roles. SetError. " + ex.Message);
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
                case "NameTextBox":
                    {
                        if (NameTextBox.Text == "Наименование")
                            NameTextBox.Text = "";

                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Roles. TextBox_GotFocus. " + ex.Message);
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
                case "NameTextBox":
                    {
                        if (NameTextBox.Text == "")
                            NameTextBox.Text = "Наименование";

                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Roles. TextBox_LostFocus. " + ex.Message);
        }
    }

    /// <summary>
    /// Метод нажатия на кнопку сохранения данных по добавлению пользователей
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            /*Отключаем элементы на странице*/
            NameTextBox.IsEnabled = false;
            SaveButton.IsEnabled = false;

            /*Убираем тест ошибки*/
            ErrorTextBlock.Text = null;

            /*Проверяем ошибки*/
            if (String.IsNullOrEmpty(NameTextBox.Text) || NameTextBox.Text == "Наименование")
            {
                SetError("Не указано наименование роли", false);
                return;
            }

            AddRoleRequest request = new(NameTextBox.Text);

            /*Если в конфиге есть данные для формирования ссылки запроса*/
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["DefaultConnection"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Api"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Roles"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Token"]))
            {
                /*Формируем ссылку запроса*/
                string url = ConfigurationManager.AppSettings["DefaultConnection"] + ConfigurationManager.AppSettings["Api"] +
                    ConfigurationManager.AppSettings["Roles"] + "add";

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

                    /*Если успешно, выводим окно об успешности*/
                    if (response.Success && response.Id != null)
                    {
                        NameTextBox.Text = "Наименование";
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
        catch (Exception ex)
        {
            SetError("Не удалось зарегистрировать полоьзователя. Обратитесь в техническую поддержку", true);
            _logger.Error("Roles. SaveButton_Click. " + ex.Message);
        }
        finally
        {
            /*Включаем элементы на странице*/
            NameTextBox.IsEnabled = true;
            SaveButton.IsEnabled = true;
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
                SaveButton_Click(sender, e);
        }
        catch (Exception ex)
        {
            _logger.Error("Roles. Enter. " + ex.Message);
        }
    }
}
