using Azure;
using Client.Controls.Administrators;
using Client.Models.Base;
using Client.Models.Geography.Countries.Response;
using Microsoft.AspNetCore.WebUtilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client.Controls.Statistics;

/// <summary>
/// Логика взаимодействия для Countries.xaml
/// </summary>
public partial class Countries : UserControl
{
    private ILogger _logger { get { return Log.ForContext<Roles>(); } } //сервис для записи логов
    private readonly JsonSerializerOptions _settings = new(); //настройки десериализации json
    private int? skip, take = 15; //пагинация
    private string? search; //строка поиска
    private bool isDeleted; //признак удалённых записей
    private List<BaseSortRequest?>? sort = new(); //список сортировки
    private ObservableCollection<CountriesResponseListItem?>? countries = new(); //коллекция стран

    /// <summary>
    /// Конструктор страницы стран
    /// </summary>
    public Countries()
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
            _logger.Error("Countries. Ошибка: {1}", ex);
        }
    }

    /// <summary>
    /// Метод проверки соединения с api
    /// </summary>
    public async void CheckConnection()
    {
        try
        {
            //Блокируем все элементы
            CountriesDataGrid.IsEnabled = false;

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
                        //Разблокируем все элементы
                        CountriesDataGrid.IsEnabled = true;

                        //Устанавливаем первоначальные параметры пагинации и сортировки
                        skip = 0;
                        sort.Add(new("number", true));

                        //Вызываем метод получения стран
                        GetCountries(null, skip, take, sort, isDeleted, false);

                        CountriesDataGrid.ItemsSource = countries;
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
            _logger.Error("Countries. SetError. Ошибка: {1}", ex);
        }
    }

    /// <summary>
    /// Метод получения стран
    /// </summary>
    public async void GetCountries(string? search, int? skip, int? take, List<BaseSortRequest?>? sort, bool isDeleted, bool clear)
    {
        try
        {
            //Объявляем переменную ссылки запроса
            string url = null;

            //Если в конфиге есть данные для формирования ссылки запроса
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["DefaultConnection"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Api"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Countries"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Token"]))
            {
                //Формируем ссылку запроса
                url = ConfigurationManager.AppSettings["DefaultConnection"] + ConfigurationManager.AppSettings["Api"] +
                    ConfigurationManager.AppSettings["Countries"] + "listFull";

                //Добавляем параметры строки
                url += CreateQueryStringListCountries(search, skip, take, sort, isDeleted);

                //Формируем клиента и добавляем токен
                using HttpClient client = new();

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ConfigurationManager.AppSettings["Token"]);

                //Получаем данные по запросу
                using var result = await client.GetAsync(url);

                //Если получили успешный результат
                if (result != null && result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    //Если указали необходимость очистки списка, очищаем его
                    if (clear)
                        countries.Clear();

                    //Десериализуем ответ и заполняем коллекцию стран
                    var content = await result.Content.ReadAsStringAsync();
                    var respose = JsonSerializer.Deserialize<CountriesResponseList>(content, _settings).Items;
                    foreach (var item in respose)
                    {
                        countries.Add(item);
                    }

                    //Если пришло меньше, чем количество запрашиваемых полей, скрываем кнопку пагинации
                    if (respose.Count < take)
                        PaginationButton.Visibility = Visibility.Hidden;
                    else
                        PaginationButton.Visibility = Visibility.Visible;

                    //Обновляем таблицу стран
                    CountriesDataGrid.Items.Refresh();
                }
                //В ином случае обрабатываем ошибки
                else
                {
                    //Если пришёл статус - Неавторизованн
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
    /// Формирование строки запроса для списка стран
    /// </summary>
    /// <returns></returns>
    public string CreateQueryStringListCountries(string? search, int? skip, int? take, List<BaseSortRequest?>? sort, bool isDeleted)
    {
        //Формируем ссылку
        string url = string.Format("?isDeleted={0}", isDeleted);

        //Если есть строка поиска добавляем в ссылку
        if (!String.IsNullOrEmpty(search))
            url += string.Format("&search={0}", search); ;

        //Если указано количество пропущенных элементов, добавляем
        if(skip != null)
            url += string.Format("&skip={0}", skip);

        //Если указано количество формируемых элементов, добавляем
        if (take != null)
            url += string.Format("&take={0}", take);

        //Если есть поля сортировки
        if (sort.Any())
        {
            //Проходим по всем полям сортировки
            for(int i = 0; i < sort.Count; i++)
            {
                //Добавляем ключ сортировки
                url += string.Format("&sort[{0}].SortKey={1}", i, sort[i].SortKey);
                //Добавляем порядок сортировки
                url += string.Format("&sort[{0}].IsAscending={1}", i, sort[i].IsAscending);
            }
        }

        //Возвращаем результат
        return url;
    }

    /// <summary>
    /// Метод пагинации на странице
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void PaginationButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            //Блокируем кнопку пагинации
            PaginationButton.IsEnabled = false;

            //Устанавливаем новое количество пропускаемых элементов
            skip += take;

            //Получаем элементы
            GetCountries(search, skip, take, sort, isDeleted, false);
        }
        catch (Exception ex)
        {
            SetError(ex.Message, true);
        }
        finally
        {
            //Разблокируем кнопку пагинации
            PaginationButton.IsEnabled = true;
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
                case "SearchTextBox":
                    {
                        if (SearchTextBox.Text == "Поиск...")
                            SearchTextBox.Text = "";

                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Countries. TextBox_GotFocus. Ошибка: {1}", ex);
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
                case "SearchTextBox":
                    {
                        if (SearchTextBox.Text == "")
                            SearchTextBox.Text = "Поиск...";

                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Countries. TextBox_LostFocus. Ошибка: {1}", ex);
        }
    }

    /// <summary>
    /// Обработка поиска по enter
    /// </summary>
    public void TextBoxEnter(object sender, KeyEventArgs e)
    {
        try
        {
            //Если нражатая клавиша - Enter
            if (e.Key == Key.Enter)
                //Вызываем метод нажатия на кнопку поиска
                SearchButton_Click(sender, e);
        }
        catch (Exception ex)
        {
            _logger.Error("Countries. TextBoxEnter. Ошибка: {1}", ex);
        }
    }

    /// <summary>
    /// Метод нажатия на поиск
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            //Если есть введённый текст, кроме дефолтного
            if (!String.IsNullOrEmpty(SearchTextBox.Text) && SearchTextBox.Text != "Поиск...")
            {
                //Устанавливаем параметры поиска
                search = SearchTextBox.Text;
                skip = 0;
                take = 15;

                //Получаем список стран
                GetCountries(search, skip, take, sort, isDeleted, true);
            }
            else
            {
                SetError("Не указано значение для поиска", false);
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Countries. SearchButton_Click. Ошибка: {1}", ex);
        }
    }

    /// <summary>
    /// Метод переключения признака удалённой записи
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void DeletedRadioButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            //Устанавливаем параметры поиска
            isDeleted = DeletedRadioButton.IsChecked ?? false;
            skip = 0;
            take = 15;

            //Получаем список стран
            GetCountries(search, skip, take, sort, isDeleted, true);
        }
        catch (Exception ex)
        {
            _logger.Error("Countries. DeletedRadioButton_Checked. Ошибка: {1}", ex);
        }
    }
}
