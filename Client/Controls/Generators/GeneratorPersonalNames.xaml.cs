using Domain.Models.Base;
using Domain.Models.Sociology.Names;
using Microsoft.AspNetCore.WebUtilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace Client.Controls.Generators;

/// <summary>
/// Логика взаимодействия для GeneratorPersonalNames.xaml
/// </summary>
public partial class GeneratorPersonalNames : UserControl
{
    private ILogger _logger { get { return Log.ForContext<GeneratorPersonalNames>(); } } //сервис для записи логов
    private readonly JsonSerializerOptions _settings = new(); //настройки десериализации json

    /// <summary>
    /// Конструктор страницы генерации личных имён
    /// </summary>
    public GeneratorPersonalNames()
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
            _logger.Error("GeneratorPersonalNames. Ошибка: {0}", ex);
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
                        //Вызываем метод получения рас
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
            _logger.Error("GeneratorPersonalNames. SetError. Ошибка: {0}", ex);
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
    /// Метод генерации имени
    /// </summary>
    public async void GetGenerateName(string nationId, bool gender, bool generateLastName)
    {
        try
        {
            //Объявляем переменную ссылки запроса
            string path = null;

            //Скрываем поля вывода
            NameTextBlock.Visibility = Visibility.Collapsed;
            NameTextBlock.Text = null;
            PrefixTextBlock.Visibility = Visibility.Collapsed;
            PrefixTextBlock.Text = null;
            LastNameTextBlock.Visibility = Visibility.Collapsed;
            LastNameTextBlock.Text = null;

            //Если в конфиге есть данные для формирования ссылки запроса
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["DefaultConnection"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Api"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["PersonalNames"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Token"]))
            {
                //Формируем ссылку запроса
                path = ConfigurationManager.AppSettings["DefaultConnection"] + ConfigurationManager.AppSettings["Api"] +
                    ConfigurationManager.AppSettings["PersonalNames"] + "generate";

                //Добавляем параметры строки
                var queryParams = new Dictionary<string, string>
                {
                    ["nationId"] = nationId,
                    ["gender"] = gender.ToString(),
                    ["generateLastName"] = generateLastName.ToString()
                };

                //Формируем клиента и добавляем токен
                using HttpClient client = new();

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ConfigurationManager.AppSettings["Token"]);

                //Получаем данные по запросу
                using var result = await client.GetAsync(QueryHelpers.AddQueryString(path, queryParams));

                //Если получили успешный результат
                if (result != null && result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    //Десериализуем ответ и заполняем поля вывода
                    var content = await result.Content.ReadAsStringAsync();

                    GeneratedName response = JsonSerializer.Deserialize<GeneratedName>(content, _settings);

                    NameTextBlock.Text = response.PersonalName;
                    NameTextBlock.Visibility = Visibility.Visible;

                    if (!String.IsNullOrEmpty(response.Prefix))
                    {
                        PrefixTextBlock.Text = response.Prefix;
                        PrefixTextBlock.Visibility = Visibility.Visible;
                    }

                    if (!String.IsNullOrEmpty(response.LastName))
                    {
                        LastNameTextBlock.Text = response.LastName;
                        LastNameTextBlock.Visibility = Visibility.Visible;
                    }
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

            //Получаем нации
            string raceId = RacesComboBox.SelectedValue.ToString();
            GetNations(raceId);

            //Включаем выпадающий список наций
            NationsComboBox.IsEnabled = true;
            NationsComboBox.Text = "Нации";
        }
        catch (Exception ex)
        {
            _logger.Error("GeneratorPersonalNames. RacesComboBox_Selected. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Событие выбора в выпадающем списке наций
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void NationsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            //Если есть выыбранный элемент
            if (NationsComboBox.SelectedValue != null)
                //Включаем кнопку генерации
                GenerateButton.IsEnabled = true;
        }
        catch (Exception ex)
        {
            _logger.Error("GeneratorPersonalNames. NationsComboBox_SelectionChanged. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Событие нажатия на кнопку сгенерировать
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void GenerateButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            //Очищаем текст ошибки
            ErrorTextBlock.Text = null;

            //Отключаем кнопку
            GenerateButton.IsEnabled = false;

            //Получаем данные для генерации
            var nationId = NationsComboBox.SelectedValue;
            bool gender = GenderRadioButton.IsChecked ?? false;
            bool generateLastName = GenerateLastNameRadioButton.IsChecked ?? false;

            //Проверяем данные
            if (nationId == null || String.IsNullOrEmpty(nationId.ToString()))
            {
                SetError("Не выбрана нация", false);
                return;
            }

            //Вызываем метод генерации имени
            GetGenerateName(nationId.ToString(), gender, generateLastName);
        }
        catch (Exception ex)
        {
            SetError(ex.Message, true);
        }
        finally
        {
            //Включаем кнопку
            GenerateButton.IsEnabled = true;
        }
    }
}
