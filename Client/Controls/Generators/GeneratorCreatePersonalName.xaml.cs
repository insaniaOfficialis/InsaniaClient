using Serilog;
using System.Net.Http;
using System.Text.Json;
using System;
using System.Windows.Controls;
using System.Windows;
using System.Configuration;
using Client.Models.Base;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Client.Controls.Bases;

namespace Client.Controls.Generators;

/// <summary>
/// Логика взаимодействия для GeneratorCreatePersonalName.xaml
/// </summary>
public partial class GeneratorCreatePersonalName : UserControl
{
    private ILogger _logger { get { return Log.ForContext<GeneratorCreatePersonalName>(); } } //сервис для записи логов
    private readonly JsonSerializerOptions _settings = new(); //настройки десериализации json

    /// <summary>
    /// Конструктор страницы генерации создания личных имён
    /// </summary>
    public GeneratorCreatePersonalName()
    {
        try
        {
            //Инициализация всех компонентов
            InitializeComponent();

            //Выставлыяем параметры десериализации
            _settings.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

            //Проверяем доступность api
            CheckConnection();
        }
        catch (Exception ex)
        {
            _logger.Error("GeneratorCreatePersonalName. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Метод проверки соединения с api
    /// </summary>
    public async void CheckConnection()
    {
        try
        {
            //Объявляем переменную ссылки запроса
            string url = null;

            try
            {
                //Если в конфиге есть данные для формирования ссылки запроса
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["DefaultConnection"]))
                {
                    //Формируем ссылку запроса
                    url = ConfigurationManager.AppSettings["DefaultConnection"];

                    //Получаем данные по запросу
                    using HttpClient client = new();

                    using var result = await client.GetAsync(url);

                    //Если получили успешный результат
                    if (result != null && result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        //Получаем расы
                        GetRaces();
                    }
                    //Иначе возвращаем ошибку
                    else
                        SetError("Сервер временно недоступен, попробуйте позднее или обратитесь в техническую поддержку", true);
                }
                //Иначе возвращаем ошибку
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
            //Объявляем переменные
            string style; //стиль

            //Определяем наименование стиля
            if (criticalException)
                style = "CriticalExceptionTextBlock";
            else
                style = "InnerExceptionTextBlock";

            //Находим стиль
            var exceptionStyle = FindResource(style) as Style;

            //Устанавливаем текст и стиль
            ErrorTextBlock.Style = exceptionStyle;
            ErrorTextBlock.Text = message;
        }
        catch (Exception ex)
        {
            _logger.Error("GeneratorCreatePersonalName. SetError. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Метод получения рас
    /// </summary>
    public async void GetRaces()
    {
        try
        {
            //Объявляем переменную ссылки запроса
            string path = null;

            //Если в конфиге есть данные для формирования ссылки запроса
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["DefaultConnection"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Api"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Races"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Token"]))
            {
                //Формируем ссылку запроса
                path = ConfigurationManager.AppSettings["DefaultConnection"] + ConfigurationManager.AppSettings["Api"] +
                    ConfigurationManager.AppSettings["Races"] + "list";

                //Формируем клиента и добавляем токен
                using HttpClient client = new();

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ConfigurationManager.AppSettings["Token"]);

                //Получаем данные по запросу
                using var result = await client.GetAsync(path);

                //Если получили успешный результат
                if (result != null && result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    //Десериализуем ответ и заполняем combobox
                    var content = await result.Content.ReadAsStringAsync();

                    BaseResponseList response = JsonSerializer.Deserialize<BaseResponseList>(content, _settings);

                    RacesComboBox.ItemsSource = response.Items;
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
    /// Метод получения наций
    /// </summary>
    public async void GetNations(string raceId)
    {
        try
        {
            //Объявляем переменную ссылки запроса
            string path = null;

            //Если в конфиге есть данные для формирования ссылки запроса
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["DefaultConnection"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Api"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Nations"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Token"]))
            {
                //Формируем ссылку запроса
                path = ConfigurationManager.AppSettings["DefaultConnection"] + ConfigurationManager.AppSettings["Api"] +
                    ConfigurationManager.AppSettings["Nations"] + "list";

                //Добавляем параметры строки
                var queryParams = new Dictionary<string, string>
                {
                    ["raceId"] = raceId
                };

                //Формируем клиента и добавляем токен
                using HttpClient client = new();

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ConfigurationManager.AppSettings["Token"]);

                //Получаем данные по запросу
                using var result = await client.GetAsync(QueryHelpers.AddQueryString(path, queryParams));

                //Если получили успешный результат
                if (result != null && result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    //Десериализуем ответ и заполняем combobox
                    var content = await result.Content.ReadAsStringAsync();

                    BaseResponseList response = JsonSerializer.Deserialize<BaseResponseList>(content, _settings);

                    NationsComboBox.ItemsSource = response.Items;
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
    /// Метод получения начал имён
    /// </summary>
    public async Task GetBeginningsNames()
    {
        try
        {
            //Объявляем переменную ссылки запроса
            string path = null;

            //Если в конфиге есть данные для формирования ссылки запроса
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["DefaultConnection"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Api"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["PersonalNames"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Token"]))
            {
                //Формируем ссылку запроса
                path = ConfigurationManager.AppSettings["DefaultConnection"] + ConfigurationManager.AppSettings["Api"] +
                    ConfigurationManager.AppSettings["PersonalNames"] + "beginningsNames";

                //Формируем клиента и добавляем токен
                using HttpClient client = new();

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ConfigurationManager.AppSettings["Token"]);

                //Получаем данные по запросу
                using var result = await client.GetAsync(path);

                //Если получили успешный результат
                if (result != null && result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    //Десериализуем ответ и заполняем combobox
                    var content = await result.Content.ReadAsStringAsync();

                    BaseResponseList response = JsonSerializer.Deserialize<BaseResponseList>(content, _settings);
                    
                    StartComboBox.IsEnabled = true;

                    StartComboBox.ItemsSource = response.Items;
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
    /// Событие выбора в выпадающем списке рас
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void RacesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            //Отключаем кнопку генерации
            GenerateButton.IsEnabled = false;
            StartComboBox.IsEnabled = false;
            StartComboBox.Text = "Начало";
            EndComboBox.IsEnabled = false;
            EndComboBox.Text = "Окончание";

            //Получаем нации
            string raceId = RacesComboBox.SelectedValue.ToString();
            GetNations(raceId);

            //Включаем выпадающий список наций
            NationsComboBox.IsEnabled = true;
            NationsComboBox.Text = "Нации";
        }
        catch (Exception ex)
        {
            _logger.Error("GeneratorCreatePersonalName. RacesComboBox_Selected. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Событие выбора в выпадающем списке наций
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void NationsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            //Если есть выыбранный элемент
            if (NationsComboBox.SelectedValue != null)
            {
                //Отключаем кнопку генерации и выпадающие списки начал и окончания и добавляем анимацию загрузки
                GenerateButton.IsEnabled = false;
                StartComboBox.IsEnabled = false;
                EndComboBox.IsEnabled = false;
                LoadCircleContentControl.Visibility = Visibility.Visible;
                LoadCircle loadCircle = new();
                LoadCircleContentControl.Content = loadCircle;

                //Получаем начала имён
                var getBegginigsName = GetBeginningsNames();

                //Получаем окончания имён

                //Включаем кнопку генерации, когда закончатся все задачи и отключаем анимацию загрузки
                await Task.WhenAll(getBegginigsName);
                GenerateButton.IsEnabled = true;                
                LoadCircleContentControl.Visibility = Visibility.Collapsed;
                LoadCircleContentControl.Content = null;
            }
        }
        catch (Exception ex)
        {
            _logger.Error("GeneratorCreatePersonalName. NationsComboBox_SelectionChanged. Ошибка: {0}", ex);
        }
    }
}