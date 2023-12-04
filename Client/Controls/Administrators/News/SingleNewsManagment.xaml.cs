using Client.Controls.Bases;
using Client.Services.Base;
using Serilog;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client.Controls.Administrators.News;

/// <summary>
/// Логика взаимодействия для SingleNewsManagment.xaml
/// </summary>
public partial class SingleNewsManagment : Window
{
    long _id; //id записи
    public ILogger _logger { get { return Log.ForContext<SingleNewsManagment>(); } } //логгер для записи логов
    readonly JsonSerializerOptions _settings = new(); //настройки десериализации json
    public IBaseService _baseService; //базовый сервис
    private LoadCircle _load = new(); //элемент загрузки

    /// <summary>
    /// Создание новости
    /// </summary>
    /// <param name="baseService"></param>
    public SingleNewsManagment(IBaseService baseService)
    {
        try
        {
            //Инициализируем компоненты
            InitializeComponent();

            //Выставляем параметры десериализации
            _settings.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

            //Получаем базовый сервис
            _baseService = baseService;

            //Проверяем доступность api
            _baseService.CheckConnection();
        }
        catch (Exception ex)
        {
            _logger.Error("SingleNewsManagment. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Редактирование новости
    /// </summary>
    /// <param name="baseService"></param>
    /// <param name="id"></param>
    /// <param name="title"></param>
    /// <param name="introduction"></param>
    /// <param name="ordinalNumber"></param>
    /// <param name="typeId"></param>
    public SingleNewsManagment(IBaseService baseService, long id, string title, string introduction, long ordinalNumber, long typeId)
        : this(baseService)
    {
        try
        {
            //Заполняем поля ввода
            TitleTextBox.Text = title;
            IntroductionTextBox.Text = introduction;
            OrdinalNumberTextBox.Text = ordinalNumber.ToString();
            TypeCombobox.SelectedItem = typeId;

            //Заполняем id
            _id = id;
        }
        catch (Exception ex)
        {
            _logger.Error("SingleNewsManagment. Ошибка: {0}", ex);
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
                case "TitleTextBox":
                    {
                        if (TitleTextBox.Text == "Заголовок")
                            TitleTextBox.Text = "";
                    }
                    break;
                case "IntroductionTextBox":
                    {
                        if (IntroductionTextBox.Text == "Вступление")
                            IntroductionTextBox.Text = "";
                    }
                    break;
                case "ColorTextBox":
                    {
                        if (OrdinalNumberTextBox.Text == "Порядковый номер")
                            OrdinalNumberTextBox.Text = "";
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.Error("SingleNewsManagment. TextBox_GotFocus. Ошибка: {0}", ex);
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
                case "TitleTextBox":
                    {
                        if (TitleTextBox.Text == "")
                            TitleTextBox.Text = "Заголовок";
                    }
                    break;
                case "IntroductionTextBox":
                    {
                        if (IntroductionTextBox.Text == "")
                            IntroductionTextBox.Text = "Вступление";
                    }
                    break;
                case "ColorTextBox":
                    {
                        if (OrdinalNumberTextBox.Text == "")
                            OrdinalNumberTextBox.Text = "Порядковый номер";
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.Error("SingleNewsManagment. TextBox_LostFocus. Ошибка: {0}", ex);
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
            var serverExceptionStyle = FindResource(style) as Style;

            //Устанавливаем текст и стиль
            ErrorText.Style = serverExceptionStyle;
            ErrorText.Text = message;

            //Включаем кнопку сохранения
            SaveButton.IsEnabled = true;
        }
        catch (Exception ex)
        {
            _logger.Error("SingleNewsManagment. SetError. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Метод сохранения
    /// </summary>
    private async void Save()
    {/*
        try
        {
            //Отключаем кнопку для нажатия
            SaveButton.IsEnabled = false;

            //Убираем тест ошибки
            ErrorText.Text = null;

            //Проверяем ошибки
            if (String.IsNullOrEmpty(RowNumberTextBox.Text) || RowNumberTextBox.Text == "Номер на карте")
            {
                SetError("Не указан номер на карте", false);
                return;
            }
            if (String.IsNullOrEmpty(NameTextBox.Text) || NameTextBox.Text == "Наименование")
            {
                SetError("Не указано наименование", false);
                return;
            }
            if (String.IsNullOrEmpty(ColorTextBox.Text) || ColorTextBox.Text == "Цвет на карте")
            {
                SetError("Не указана цвет на карте", false);
                return;
            }
            if (ColorTextBox.Text.Length != 7 || !ColorTextBox.Text.StartsWith("#"))
            {
                SetError("Некорректно указан цвет на карте", false);
                return;
            }
            if (String.IsNullOrEmpty(LanguageForNamesTextBox.Text) || LanguageForNamesTextBox.Text == "Язык для названий")
            {
                SetError("Не указан язык для названий", false);
                return;
            }

            AddCountryRequest request = new(NameTextBox.Text, Convert.ToInt32(RowNumberTextBox.Text), ColorTextBox.Text, LanguageForNamesTextBox.Text);

            //Если в конфиге есть данные для формирования ссылки запроса
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["DefaultConnection"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Api"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Countries"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Token"]))
            {
                //Формируем ссылку запроса
                string url = ConfigurationManager.AppSettings["DefaultConnection"] + ConfigurationManager.AppSettings["Api"] +
                    ConfigurationManager.AppSettings["Countries"];

                if (_id <= 0)
                    url += "add";
                else
                    url += "update/" + _id;

                //Формируем клиента, добавляем ему токен и тело запроса
                using HttpClient client = new();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ConfigurationManager.AppSettings["Token"]);
                StringContent stringCintent = new(JsonSerializer.Serialize(request, _settings).ToString(), Encoding.UTF8, "application/json");

                //Получаем результат запроса
                HttpResponseMessage result;
                if (_id <= 0)
                    result = await client.PostAsync(url, stringCintent);
                else
                    result = await client.PutAsync(url, stringCintent);

                //Если получили успешный результат
                if (result != null)
                {
                    if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        SetError("Некорректный токен", false);

                    //Десериализуем ответ
                    var content = await result.Content.ReadAsStringAsync();

                    BaseResponse response = JsonSerializer.Deserialize<BaseResponse>(content, _settings);

                    //Если успешно, зыкрываем окно
                    if (response.Success && response.Id != null)
                    {
                        Close();
                        Message message = new("Успешно");
                        message.Show();
                    }
                    else
                        SetError(response?.Error?.Message ?? "Ошибка сервера", false);
                }
                else
                    SetError("Ошибка сервера", true);
            }
            //Иначе возвращаем ошибку
            else
                SetError("Не указаны адреса api. Обратитесь в техническую поддержку", true);

            //Включаем кнопку для нажатия
            SaveButton.IsEnabled = true;
        }
        catch (Exception ex)
        {
            SetError("Не удалось сохранить. Обратитесь в техническую поддержку", true);
            _logger.Error("SingleNewsManagment. Save. Ошибка: {0}", ex);
        }*/
    }

    /// <summary>
    /// Событие нажатия клавиши сохранить
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            //Вызываем сохранение
            Save();
        }
        catch (Exception ex)
        {
            _logger.Error("SingleNewsManagment. SaveButton_Click. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Событие нажатия клавиш
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        try
        {
            //Если нажата клавиша eacape
            if (e.Key == Key.Escape)
                //Закрываем окно
                Close();

            //Если нажата клавиша enter
            if (e.Key == Key.Enter)
                //Вызываем сохранение
                Save();
        }
        catch (Exception ex)
        {
            _logger.Error("SingleNewsManagment. Window_PreviewKeyDown. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Событие перетягивания мыши
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            //Если левая кнопка мыши
            if (e.ChangedButton == MouseButton.Left)
                //Включаем перетягивание
                DragMove();
        }
        catch (Exception ex)
        {
            _logger.Error("SingleNewsManagment. Window_MouseDown. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Получение типов
    /// </summary>
    public async Task GetTypes()
    {
        /*try
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
        }*/
    }

    /// <summary>
    /// Событие загрузки окна
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            //Включаем элемент загрузки
            LoadContent.Content = _load;
            LoadContent.Visibility = Visibility.Visible;

            //Ставим сортировки по умолчанию
            /*_sort.Add(new("id", false));
            NewsDataGrid.Items.SortDescriptions.Add(new SortDescription("Id", ListSortDirection.Descending));

            //Получаем новости
            var response = await _getNewsTable.Handler(_search, _skip, _take, _sort, _isDeleted);

            //Наполняем коллекцию новостей
            if (response != null && response.Items.Any())
            {
                foreach (var item in response.Items)
                {
                    if (_accessRightAction != null)
                    {
                        item.Edit = _accessRightAction.Edit;
                        item.Delete = _accessRightAction.Delete;
                        item.Restore = _accessRightAction.Restore;
                    }
                    _news.Add(item);
                }
            }

            //Обновляем таблицу
            NewsDataGrid.ItemsSource = _news;

            //Обрабываем видимость кнопки пагинации
            if (_news.Count > 19)
                PaginationButton.Visibility = Visibility.Visible;
            else
                PaginationButton.Visibility = Visibility.Collapsed;*/
        }
        catch (Exception ex)
        {
            SetError(ex.Message, true);
        }
        finally
        {
            //Отключаем элемент загрузки
            LoadContent.Content = null;
            LoadContent.Visibility = Visibility.Visible;
        }
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {

    }

    private void DeletedButton_Click(object sender, RoutedEventArgs e)
    {

    }

    private void RestoredButton_Click(object sender, RoutedEventArgs e)
    {

    }
}