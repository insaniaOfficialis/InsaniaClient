using Client.Controls.Bases;
using Client.Models.Base;
using Client.Services.Base;
using Microsoft.AspNetCore.WebUtilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Configuration;
using System.Text.Json;
using System.Windows.Input;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace Client.Controls.Administrators;

/// <summary>
/// Логика взаимодействия для CreatePersonalName.xaml
/// </summary>
public partial class CreatePersonalName : UserControl
{
    private ILogger _logger { get { return Log.ForContext<CreatePersonalName>(); } } //сервис для записи логов
    private LoadCircle _loadCircle = new(); //анимацияч загрузки
    public IBaseService _baseService; //базовый сервис

    /// <summary>
    /// Конструктор страницы создания личных имён
    /// </summary>
    public CreatePersonalName (IBaseService baseService)
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
            _logger.Error("CreatePersonalName. Ошибка: {0}", ex);
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
            _logger.Error("CreatePersonalName. SetError. Ошибка: {0}", ex);
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
            string url = null;

            //Если в конфиге есть данные для формирования ссылки запроса
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["DefaultConnection"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Api"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Races"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Token"]))
            {
                //Формируем ссылку запроса
                url = ConfigurationManager.AppSettings["DefaultConnection"] + ConfigurationManager.AppSettings["Api"] +
                    ConfigurationManager.AppSettings["Races"] + "list";

                //Формируем клиента и добавляем токен
                using HttpClient client = new();

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ConfigurationManager.AppSettings["Token"]);

                //Получаем данные по запросу
                using var result = await client.GetAsync(url);

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
            string url = null;

            //Если в конфиге есть данные для формирования ссылки запроса
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["DefaultConnection"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Api"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Nations"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Token"]))
            {
                //Формируем ссылку запроса
                url = ConfigurationManager.AppSettings["DefaultConnection"] + ConfigurationManager.AppSettings["Api"] +
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
                using var result = await client.GetAsync(QueryHelpers.AddQueryString(url, queryParams));

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
    /// Метод сохранения имени
    /// </summary>
    /// <param name="nationId"></param>
    /// <param name="gender"></param>
    /// <param name="name"></param>
    /// <param name="probability"></param>
    /// <returns></returns>
    public async Task<bool> SaveName(string nationId, string gender, string name, string probability)
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
                    ["name"] = name,
                    ["probability"] = probability
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
                if (result != null)
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
                        return true;
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

            return false;
        }
        catch (Exception ex)
        {
            SetError(ex.Message, true);
            return false;
        }
    }

    /// <summary>
    /// Обработка сохранения по enter
    /// </summary>
    public void Enter(object sender, KeyEventArgs e)
    {
        try
        {
            //Если нажата кнопка enter
            if (e.Key == Key.Enter)
                //Вызываем метод сохранения
                SaveButton_Click(sender, e);
        }
        catch (Exception ex)
        {
            _logger.Error("CreatePersonalName. Enter. Ошибка: {0}", ex);
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
                        if (NameTextBox.Text == "Имя")
                            NameTextBox.Text = "";
                    }
                    break;
                case "ProbabilityTextBox":
                    {
                        if (ProbabilityTextBox.Text == "Вероятность")
                            ProbabilityTextBox.Text = "";
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.Error("CreatePersonalName. TextBox_GotFocus. Ошибка: {0}", ex);
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
                            NameTextBox.Text = "Имя";
                    }
                    break;
                case "ProbabilityTextBox":
                    {
                        if (ProbabilityTextBox.Text == "")
                            ProbabilityTextBox.Text = "Вероятность";
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.Error("CreatePersonalName. TextBox_LostFocus. Ошибка: {0}", ex);
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
            //Если есть выыбранный элемент
            if (RacesComboBox.SelectedValue != null)
            {
                //Отключаем элементы управления и включаем анимацию загрузки
                SaveButton.IsEnabled = false;
                RacesComboBox.IsEnabled = false;
                NationsComboBox.IsEnabled = false;
                GenderRadioButton.IsEnabled = false;
                LoadCircleContentControl.Visibility = Visibility.Visible;
                LoadCircleContentControl.Content = _loadCircle;

                //Получаем нации
                string raceId = RacesComboBox.SelectedValue.ToString();
                await GetNations(raceId);

                //Включаем элементы управления, когда закончатся все задачи и отключаем анимацию загрузки
                RacesComboBox.IsEnabled = true;
                NationsComboBox.IsEnabled = true;
                NationsComboBox.Text = "Нации";
                GenderRadioButton.IsEnabled = true;
                LoadCircleContentControl.Visibility = Visibility.Collapsed;
                LoadCircleContentControl.Content = null;
            }
        }
        catch (Exception ex)
        {
            _logger.Error("CreatePersonalName. RacesComboBox_Selected. Ошибка: {0}", ex);
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
            {
                //Включаем элементы управления
                SaveButton.IsEnabled = true;
                RacesComboBox.IsEnabled = true;
                NationsComboBox.IsEnabled = true;
                GenderRadioButton.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            _logger.Error("CreatePersonalName. NationsComboBox_SelectionChanged. Ошибка: {0}", ex);
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
            SaveButton.IsEnabled = false;
            RacesComboBox.IsEnabled = false;
            NationsComboBox.IsEnabled = false;
            GenderRadioButton.IsEnabled = false;
            LoadCircleContentControl.Visibility = Visibility.Visible;
            LoadCircleContentControl.Content = _loadCircle;

            //Получаем нацию, пол и сгенерированное имя
            string nationId = NationsComboBox.SelectedValue.ToString();
            string gender = GenderRadioButton.IsChecked.ToString();
            string name = NameTextBox.Text;
            string probability = ProbabilityTextBox.Text;

            //Проверяем корректность данных
            if (string.IsNullOrEmpty(nationId))
            {
                SaveButton.IsEnabled = true;
                RacesComboBox.IsEnabled = true;
                NationsComboBox.IsEnabled = true;
                GenderRadioButton.IsEnabled = true;
                LoadCircleContentControl.Visibility = Visibility.Collapsed;
                LoadCircleContentControl.Content = null;
                SetError("Не указана нация", false);
                return;
            }
            if (string.IsNullOrEmpty(gender))
            {
                SaveButton.IsEnabled = true;
                RacesComboBox.IsEnabled = true;
                NationsComboBox.IsEnabled = true;
                GenderRadioButton.IsEnabled = true;
                LoadCircleContentControl.Visibility = Visibility.Collapsed;
                LoadCircleContentControl.Content = null;
                SetError("Не указан пол", false);
                return;
            }
            if (string.IsNullOrEmpty(name) || name == "Имя")
            {
                SaveButton.IsEnabled = true;
                RacesComboBox.IsEnabled = true;
                NationsComboBox.IsEnabled = true;
                GenderRadioButton.IsEnabled = true;
                LoadCircleContentControl.Visibility = Visibility.Collapsed;
                LoadCircleContentControl.Content = null;
                SetError("Не указано имя", false);
                return;
            }
            if (string.IsNullOrEmpty(probability) || probability == "Вероятность")
            {
                SaveButton.IsEnabled = true;
                RacesComboBox.IsEnabled = true;
                NationsComboBox.IsEnabled = true;
                GenderRadioButton.IsEnabled = true;
                LoadCircleContentControl.Visibility = Visibility.Collapsed;
                LoadCircleContentControl.Content = null;
                SetError("Не указана вероятность", false);
                return;
            }
            if(!double.TryParse(probability, out double probabilityDouble))
            {
                SaveButton.IsEnabled = true;
                RacesComboBox.IsEnabled = true;
                NationsComboBox.IsEnabled = true;
                GenderRadioButton.IsEnabled = true;
                LoadCircleContentControl.Visibility = Visibility.Collapsed;
                LoadCircleContentControl.Content = null;
                SetError("Некорректно указана вероятность", false);
                return;
            }

            //Сохраняем сгенерированное имя
            var result = await SaveName(nationId, gender, name, probability.Replace(",", "."));

            //Если реузльтат успешный
            if (result)
            {
                //Включаем элементы управления, очищаем данные и отключаем анимацию загрузки
                RacesComboBox.IsEnabled = true;
                GenderRadioButton.IsEnabled = true;
                NameTextBox.Text = "Имя";
                ProbabilityTextBox.Text = "Вероятность";
                RacesComboBox.SelectedItem = null;
                RacesComboBox.Text = "Расы";
                NationsComboBox.SelectedItem = null;
                NationsComboBox.Text = "Нации";
                ErrorTextBlock.Text = null;
                LoadCircleContentControl.Visibility = Visibility.Collapsed;
                LoadCircleContentControl.Content = null;
            }
            //Иначе
            else
            {
                //Включаем элементы управления и отключаем анимацию загрузки
                SaveButton.IsEnabled = true;
                RacesComboBox.IsEnabled = true;
                NationsComboBox.IsEnabled = true;
                GenderRadioButton.IsEnabled = true;
                LoadCircleContentControl.Visibility = Visibility.Collapsed;
                LoadCircleContentControl.Content = null;
            }
        }
        catch (Exception ex)
        {
            _logger.Error("CreatePersonalName. SaveButton_Click. Ошибка: {0}", ex);
        }
    }
}
