using Client.Controls.Bases;
using Client.Models.Base;
using Client.Models.Sociology.Names;
using Client.Services.Base;
using Microsoft.AspNetCore.WebUtilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Client.Controls.Generators;

/// <summary>
/// Логика взаимодействия для GeneratorCreatePersonalName.xaml
/// </summary>
public partial class GeneratorCreatePersonalName : UserControl
{
    private ILogger _logger { get { return Log.ForContext<GeneratorCreatePersonalName>(); } } //сервис для записи логов
    private LoadCircle _loadCircle = new(); //анимацияч загрузки
    public IBaseService _baseService; //базовый сервис

    /// <summary>
    /// Конструктор страницы генерации создания личных имён
    /// </summary>
    public GeneratorCreatePersonalName(IBaseService baseService)
    {
        try
        {
            //Инициализация всех компонентов
            InitializeComponent();

            //Получаем базовый сервис
            _baseService = baseService;

            //Проверяем доступность api
            _baseService.CheckConnection();

            //Получаем роли
            _ = GetRaces();
        }
        catch (Exception ex)
        {
            _logger.Error("GeneratorCreatePersonalName. Ошибка: {0}", ex);
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
    public async Task GetRaces()
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

                    BaseResponseList response = JsonSerializer
                        .Deserialize<BaseResponseList>(content, _baseService.GetJsonSettings());

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
    public async Task GetNations(string raceId)
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

                    BaseResponseList response = JsonSerializer
                        .Deserialize<BaseResponseList>(content, _baseService.GetJsonSettings());

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
    public async Task GetBeginningsNames(string nationId, string gender)
    {
        try
        {
            //Объявляем переменную ссылки запроса
            string url = null;

            //Если в конфиге есть данные для формирования ссылки запроса
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["DefaultConnection"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Api"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["PersonalNames"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Token"]))
            {
                //Формируем ссылку запроса
                url = ConfigurationManager.AppSettings["DefaultConnection"] + ConfigurationManager.AppSettings["Api"] +
                    ConfigurationManager.AppSettings["PersonalNames"] + "beginnings";

                //Добавляем параметры строки
                var queryParams = new Dictionary<string, string>
                {
                    ["nationId"] = nationId,
                    ["gender"] = gender
                };

                //Формируем клиента и добавляем токен
                using HttpClient client = new();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                    ConfigurationManager.AppSettings["Token"]);

                //Получаем данные по запросу
                using var result = await client.GetAsync(QueryHelpers.AddQueryString(url, queryParams));

                //Если получили успешный результат
                if (result != null && result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    //Десериализуем ответ и заполняем combobox
                    var content = await result.Content.ReadAsStringAsync();
                    BaseResponseList response = JsonSerializer
                        .Deserialize<BaseResponseList>(content, _baseService.GetJsonSettings());
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
    /// Метод получения окончания имён
    /// </summary>
    public async Task GetEndingsNames(string nationId, string gender)
    {
        try
        {
            //Объявляем переменную ссылки запроса
            string url = null;

            //Если в конфиге есть данные для формирования ссылки запроса
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["DefaultConnection"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Api"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["PersonalNames"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Token"]))
            {
                //Формируем ссылку запроса
                url = ConfigurationManager.AppSettings["DefaultConnection"] + ConfigurationManager.AppSettings["Api"] +
                    ConfigurationManager.AppSettings["PersonalNames"] + "endings";

                //Добавляем параметры строки
                var queryParams = new Dictionary<string, string>
                {
                    ["nationId"] = nationId,
                    ["gender"] = gender
                };

                //Формируем клиента и добавляем токен
                using HttpClient client = new();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                    ConfigurationManager.AppSettings["Token"]);

                //Получаем данные по запросу
                using var result = await client.GetAsync(QueryHelpers.AddQueryString(url, queryParams));

                //Если получили успешный результат
                if (result != null && result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    //Десериализуем ответ и заполняем combobox
                    var content = await result.Content.ReadAsStringAsync();
                    BaseResponseList response = JsonSerializer
                        .Deserialize<BaseResponseList>(content, _baseService.GetJsonSettings());

                    EndComboBox.ItemsSource = response.Items;
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
    /// Метод получения сгенерированного имени
    /// </summary>
    /// <param name="nationId"></param>
    /// <param name="gender"></param>
    /// <param name="firstSyllable"></param>
    /// <param name="lastSyllable"></param>
    /// <returns></returns>
    public async Task GetGeneratingNewName(string nationId, string gender, string? firstSyllable, string? lastSyllable)
    {
        try
        {
            //Очищаем выведенное имя
            NameTextBlock.Text = null;
            
            //Блоикруем кнопку сохранения
            SaveButton.IsEnabled = false;

            //Объявляем переменную ссылки запроса
            string url = null;

            //Если в конфиге есть данные для формирования ссылки запроса
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["DefaultConnection"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Api"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["PersonalNames"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Token"]))
            {
                //Формируем ссылку запроса
                url = ConfigurationManager.AppSettings["DefaultConnection"] + ConfigurationManager.AppSettings["Api"] +
                    ConfigurationManager.AppSettings["PersonalNames"] + "generateNew";

                //Добавляем параметры строки
                var queryParams = new Dictionary<string, string>
                {
                    ["nationId"] = nationId,
                    ["gender"] = gender
                };

                //Если указан слог начала, добавляем в колллекцию параметров строки
                if (!string.IsNullOrEmpty(firstSyllable))
                    queryParams.Add("firstSyllable", firstSyllable);

                //Если указан слог окончания, добавляем в колллекцию параметров строки
                if (!string.IsNullOrEmpty(lastSyllable))
                    queryParams.Add("lastSyllable", lastSyllable);

                //Формируем клиента и добавляем токен
                using HttpClient client = new();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                    ConfigurationManager.AppSettings["Token"]);

                //Получаем данные по запросу
                using var result = await client.GetAsync(QueryHelpers.AddQueryString(url, queryParams));

                //Если получили успешный результат
                if (result != null && result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    //Десериализуем ответ
                    var content = await result.Content.ReadAsStringAsync();
                    GeneratedName response = JsonSerializer
                        .Deserialize<GeneratedName>(content, _baseService.GetJsonSettings());

                    //Если не было ошибки, отображаем и заполняем имя, и отображаем разблокированную кнопку сохранения
                    if (response.Success)
                    {
                        NameTextBlock.Visibility = Visibility.Visible;
                        NameTextBlock.Text = response.PersonalName;
                        SaveButton.IsEnabled = true;
                        SaveButton.Visibility = Visibility.Visible;
                    }
                    //Иначе отображаем ошибку
                    else
                        SetError(response.Error.Message, true);
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
    /// Метод сохранения имени
    /// </summary>
    /// <param name="nationId"></param>
    /// <param name="gender"></param>
    /// <param name="firstSyllable"></param>
    /// <param name="lastSyllable"></param>
    /// <returns></returns>
    public async Task SaveName(string nationId, string gender, string? name)
    {
        try
        {
            //Блоикруем кнопку сохранения
            SaveButton.IsEnabled = false;

            //Объявляем переменную ссылки запроса
            string url = null;

            //Если в конфиге есть данные для формирования ссылки запроса
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["DefaultConnection"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Api"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["PersonalNames"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Token"]))
            {
                //Формируем ссылку запроса
                url = ConfigurationManager.AppSettings["DefaultConnection"] + ConfigurationManager.AppSettings["Api"] +
                    ConfigurationManager.AppSettings["PersonalNames"];

                //Добавляем параметры строки
                var queryParams = new Dictionary<string, string>
                {
                    ["nationId"] = nationId,
                    ["gender"] = gender,
                    ["name"] = name
                };

                //Формируем клиента и добавляем токен
                using HttpClient client = new();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                    ConfigurationManager.AppSettings["Token"]);

                //Формируем строку с параметрами
                url = QueryHelpers.AddQueryString(url, queryParams);

                //Получаем данные по запросу
                using var result = await client.PostAsync(url, null);

                //Если получили успешный результат
                if (result != null && result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    //Десериализуем ответ
                    var content = await result.Content.ReadAsStringAsync();
                    BaseResponse response = JsonSerializer
                        .Deserialize<BaseResponse>(content, _baseService.GetJsonSettings());

                    //Если не было ошибки, отображаем окно об успешности
                    if (response.Success)
                    {
                        Message message = new("Успешно");
                        message.Show();
                    }
                    //Иначе отображаем ошибку
                    else
                        SetError(response.Error.Message, true);
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
    private async void RacesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            //Отключаем элементы управления и включаем анимацию загрузки
            GenerateButton.IsEnabled = false;
            StartComboBox.IsEnabled = false;
            StartComboBox.Text = "Начало";
            EndComboBox.IsEnabled = false;
            EndComboBox.Text = "Окончание";
            LoadCircleContentControl.Visibility = Visibility.Visible;
            LoadCircleContentControl.Content = _loadCircle;

            //Получаем нации
            string raceId = RacesComboBox.SelectedValue.ToString();
            await GetNations(raceId);

            //Включаем элементы управления, когда закончатся все задачи и отключаем анимацию загрузки
            NationsComboBox.IsEnabled = true;
            NationsComboBox.Text = "Нации";
            LoadCircleContentControl.Visibility = Visibility.Collapsed;
            LoadCircleContentControl.Content = null;
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
                //Отключаем элементы управления и добавляем анимацию загрузки
                GenerateButton.IsEnabled = false;
                StartComboBox.IsEnabled = false;
                EndComboBox.IsEnabled = false;
                GenderRadioButton.IsEnabled = false;
                LoadCircleContentControl.Visibility = Visibility.Visible;
                LoadCircleContentControl.Content = _loadCircle;
                
                //Получаем нацию и пол
                string nationId = NationsComboBox.SelectedValue.ToString();
                string gender = GenderRadioButton.IsChecked.ToString();

                //Получаем начала имён
                var getBegginigsName = GetBeginningsNames(nationId, gender);

                //Получаем окончания имён
                var getEndingsNames = GetEndingsNames(nationId, gender);

                //Включаем элементы управления, когда закончатся все задачи и отключаем анимацию загрузки
                await Task.WhenAll(getBegginigsName, getEndingsNames);
                GenerateButton.IsEnabled = true;
                StartComboBox.IsEnabled = true;
                EndComboBox.IsEnabled = true;
                GenderRadioButton.IsEnabled = true;
                LoadCircleContentControl.Visibility = Visibility.Collapsed;
                LoadCircleContentControl.Content = null;
            }
        }
        catch (Exception ex)
        {
            _logger.Error("GeneratorCreatePersonalName. NationsComboBox_SelectionChanged. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Событие нажатия на переключатель пола
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void GenderRadioButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            //Отключаем элементы управления и добавляем анимацию загрузки
            GenerateButton.IsEnabled = false;
            RacesComboBox.IsEnabled = false;
            NationsComboBox.IsEnabled = false;
            StartComboBox.IsEnabled = false;
            EndComboBox.IsEnabled = false;
            GenderRadioButton.IsEnabled = false;
            LoadCircleContentControl.Visibility = Visibility.Visible;
            LoadCircleContentControl.Content = _loadCircle;

            //Получаем нацию и пол
            string nationId = NationsComboBox.SelectedValue.ToString();
            string gender = GenderRadioButton.IsChecked.ToString();

            //Получаем начала имён
            var getBegginigsName = GetBeginningsNames(nationId, gender);

            //Получаем окончания имён
            var getEndingsNames = GetEndingsNames(nationId, gender);

            //Включаем элементы управления, когда закончатся все задачи и отключаем анимацию загрузки
            await Task.WhenAll(getBegginigsName, getEndingsNames);
            GenerateButton.IsEnabled = true;
            RacesComboBox.IsEnabled = true;
            NationsComboBox.IsEnabled = true;
            StartComboBox.IsEnabled = true;
            StartComboBox.Text = "Начало";
            EndComboBox.IsEnabled = true;
            EndComboBox.Text = "Окончание";
            GenderRadioButton.IsEnabled = true;
            LoadCircleContentControl.Visibility = Visibility.Collapsed;
            LoadCircleContentControl.Content = null;
        }
        catch (Exception ex)
        {
            _logger.Error("GeneratorCreatePersonalName. GenderRadioButton_Checked. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Событие нажатия на кнопку "Сгенерировать"
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void GenerateButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            //Отключаем элементы управления и добавляем анимацию загрузки
            GenerateButton.IsEnabled = false;
            RacesComboBox.IsEnabled = false;
            NationsComboBox.IsEnabled = false;
            StartComboBox.IsEnabled = false;
            EndComboBox.IsEnabled = false;
            GenderRadioButton.IsEnabled = false;
            LoadCircleContentControl.Visibility = Visibility.Visible;
            LoadCircleContentControl.Content = _loadCircle;

            //Получаем нацию, пол и выбранные слоги
            string nationId = NationsComboBox.SelectedValue.ToString();
            string gender = GenderRadioButton.IsChecked.ToString();
            string? firstSyllable = null;
            string? lastSyllable = null;
            if (StartComboBox.SelectedValue != null)
                firstSyllable = StartComboBox.SelectedValue.ToString();
            if (EndComboBox.SelectedValue != null)
                lastSyllable = EndComboBox.SelectedValue.ToString();

            //Получаем сгенерированное имя
            await GetGeneratingNewName(nationId, gender, firstSyllable, lastSyllable);

            //Включаем элементы управления и отключаем анимацию загрузки
            GenerateButton.IsEnabled = true;
            RacesComboBox.IsEnabled = true;
            NationsComboBox.IsEnabled = true;
            StartComboBox.IsEnabled = true;
            EndComboBox.IsEnabled = true;
            GenderRadioButton.IsEnabled = true;
            LoadCircleContentControl.Visibility = Visibility.Collapsed;
            LoadCircleContentControl.Content = null;
        }
        catch (Exception ex)
        {
            _logger.Error("GeneratorCreatePersonalName. GenerateButton_Click. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Метод нажатия на кнопку "Сохранить"
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            //Отключаем элементы управления и добавляем анимацию загрузки
            GenerateButton.IsEnabled = false;
            RacesComboBox.IsEnabled = false;
            NationsComboBox.IsEnabled = false;
            StartComboBox.IsEnabled = false;
            EndComboBox.IsEnabled = false;
            GenderRadioButton.IsEnabled = false;
            LoadCircleContentControl.Visibility = Visibility.Visible;
            LoadCircleContentControl.Content = _loadCircle;

            //Получаем нацию, пол и сгенерированное имя
            string nationId = NationsComboBox.SelectedValue.ToString();
            string gender = GenderRadioButton.IsChecked.ToString();
            string name = NameTextBlock.Text;

            //Сохраняем сгенерированное имя
            await SaveName(nationId, gender, name);

            //Включаем элементы управления и отключаем анимацию загрузки
            GenerateButton.IsEnabled = true;
            RacesComboBox.IsEnabled = true;
            NationsComboBox.IsEnabled = true;
            StartComboBox.IsEnabled = true;
            EndComboBox.IsEnabled = true;
            GenderRadioButton.IsEnabled = true;
            LoadCircleContentControl.Visibility = Visibility.Collapsed;
            LoadCircleContentControl.Content = null;
        }
        catch (Exception ex)
        {
            _logger.Error("GeneratorCreatePersonalName. SaveButton_Click. Ошибка: {0}", ex);
        }
    }
}