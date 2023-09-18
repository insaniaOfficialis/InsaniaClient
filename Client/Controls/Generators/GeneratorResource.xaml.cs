using Client.Controls.Administrators;
using Client.Models.Base;
using Serilog;
using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client.Controls.Generators;

/// <summary>
/// Логика взаимодействия для GeneratorResource.xaml
/// </summary>
public partial class GeneratorResource : UserControl
{
    public ILogger _logger { get { return Log.ForContext<GeneratorResource>(); } } //логгер для записи логов
    readonly JsonSerializerOptions _settings = new(); //настройки десериализации json

    /// <summary>
    /// Конструктор страницы генератора ресурсов
    /// </summary>
    public GeneratorResource()
    {
        try
        {
            /*Инициализирум список*/
            InitializeComponent();

            /*Выставляем параметры десериализации*/
            _settings.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

            /*Проверяем соединение*/
            CheckConnection();
        }
        catch (Exception ex)
        {
            _logger.Error("GeneratorResource. " + ex.Message);
        }
    }

    /// <summary>
    /// Обработка генерации по enter
    /// </summary>
    public void Enter(object sender, KeyEventArgs e)
    {
        try
        {
            /*Если нажата кнопка enter*/
            if (e.Key == Key.Enter)
                /*Вызываем метод генерации*/
                GenerateButton_Click(sender, e);
        }
        catch (Exception ex)
        {
            _logger.Error("GeneratorResource. Enter. " + ex.Message);
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
            CountriesComboBox.IsEnabled = false;
            GenerateButton.IsEnabled = false;

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
                        CountriesComboBox.IsEnabled = true;
                        GenerateButton.IsEnabled = true;

                        /*Заполняем страны*/
                        GetCountries();
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
            _logger.Error("GeneratorResource. SetError. " + ex.Message);
        }
    }

    /// <summary>
    /// Метод получения стран
    /// </summary>
    public async void GetCountries()
    {
        try
        {
            /*Объявляем переменную ссылки запроса*/
            string path = null;

            /*Если в конфиге есть данные для формирования ссылки запроса*/
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["DefaultConnection"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Api"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Countries"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Token"]))
            {
                /*Формируем ссылку запроса*/
                path = ConfigurationManager.AppSettings["DefaultConnection"] + ConfigurationManager.AppSettings["Api"] +
                    ConfigurationManager.AppSettings["Countries"] + "list";

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

                    CountriesComboBox.ItemsSource = response.Items;
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
        catch (Exception ex)
        {
            SetError(ex.Message, true);
        }
    }

    /// <summary>
    /// Событие нажатия на кнопку генерации
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void GenerateButton_Click(object sender, RoutedEventArgs e)
    {

    }
}
