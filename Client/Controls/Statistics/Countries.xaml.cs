using Client.Controls.Administrators;
using Client.Controls.Statistics.Windows;
using Client.Models.Base;
using Client.Models.Politics.Countries.Response;
using DevExpress.Mvvm.Native;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    Country _country; //окно страны

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
            _logger.Error("Countries. Ошибка: {0}", ex);
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
            _logger.Error("Countries. SetError. Ошибка: {0}", ex);
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
            _logger.Error("Countries. TextBox_GotFocus. Ошибка: {0}", ex);
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
            _logger.Error("Countries. TextBox_LostFocus. Ошибка: {0}", ex);
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
            _logger.Error("Countries. TextBoxEnter. Ошибка: {0}", ex);
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
            _logger.Error("Countries. SearchButton_Click. Ошибка: {0}", ex);
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

            //Меняем видимость кнопок
            if (isDeleted)
            {
                Deleted.Visibility = Visibility.Collapsed;
                Restored.Visibility = Visibility.Visible;
            }
            else
            {
                Deleted.Visibility = Visibility.Visible;
                Restored.Visibility = Visibility.Collapsed;
            }

            //Получаем список стран
            GetCountries(search, skip, take, sort, isDeleted, true);
        }
        catch (Exception ex)
        {
            _logger.Error("Countries. DeletedRadioButton_Checked. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Метод сортировки стран
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CountriesDataGrid_Sorting(object sender, DataGridSortingEventArgs e)
    {
        try
        {
            //Объявляем переменные сортировки
            string sortKey = String.Empty;
            bool? isAscending;

            //Если у нас есть поля для сортировки
            if (!String.IsNullOrEmpty(e.Column.SortMemberPath.ToString()))
            {
                //Получаем наименование сортируемого поля
                sortKey = e.Column.SortMemberPath.ToString().ToLower();

                //Получаем порядок сортировки
                isAscending = e.Column.SortDirection == null ? true : e.Column.SortDirection == ListSortDirection.Ascending ? false : null;

                //Если не был зажат shift
                if ((Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.Shift)
                {
                    //Очищаем сортировку
                    sort.Clear();
                }

                //Если уже есть такое поле сортировки
                if (sort.Any(x => x.SortKey == sortKey))
                {
                    //Если порядок сортировки исчезает
                    if (isAscending == null)
                        //Убираем из полей сортировки
                        sort.RemoveAll(x => x.SortKey == sortKey);
                    //Иначе
                    else
                        //Меняем порядлок сортировки
                        sort.Where(x => x.SortKey == sortKey && x.IsAscending != isAscending).ForEach(x => x.IsAscending = isAscending);
                }
                //Иначе
                else
                {
                    //Если есть порядок сортировки
                    if (isAscending != null)
                        //Добавляем в сортировку новое поле
                        sort.Add(new(sortKey, isAscending ?? false));
                }

                //Обнуляем пагинацию
                skip = 0;
                take = 15;

                //Получаем страны
                GetCountries(search, skip, take, sort, isDeleted, true);
            }
        }
        catch(Exception ex)
        {
            _logger.Error("Countries. CountriesDataGrid_Sorting. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// События нажатия на кнопку добавления стран
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            //Создаём пустое окно страны
            _country = new();

            //Отображаем окно страны
            _country.ShowDialog();

            //Обновляем страны
            GetCountries(search, 0, skip + take, sort, isDeleted, true);
        }
        catch (Exception ex)
        {
            _logger.Error("Countries. AddButton_Click. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Событие нажатия на кнопку редактирвоания страны
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            //Получаем выбранную строку
            CountriesResponseListItem item = CountriesDataGrid.SelectedItem as CountriesResponseListItem;

            //Создаём заполненное окно страны
            _country = new(item.Id ?? 0, item.Number, item.Name, item.Color, item.LanguageForNames);

            //Отображаем окно страны
            _country.ShowDialog();

            //Обновляем страны
            GetCountries(search, 0, skip + take, sort, isDeleted, true);
        }
        catch (Exception ex)
        {
            _logger.Error("Countries. EditButton_Click. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Событие нажатия на кнопку удаления стран
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void DeletedButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            //Получаем выбранную строку
            CountriesResponseListItem item = CountriesDataGrid.SelectedItem as CountriesResponseListItem;

            //Блокируем таблицу
            CountriesDataGrid.IsEnabled = false;

            //Проверяем ошибки
            if (item.Id == null)
            {
                SetError("Не указан id страны", false);
                return;
            }

            //Если в конфиге есть данные для формирования ссылки запроса
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["DefaultConnection"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Api"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Countries"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Token"]))
            {
                //Формируем ссылку запроса
                string url = ConfigurationManager.AppSettings["DefaultConnection"] + ConfigurationManager.AppSettings["Api"] +
                    ConfigurationManager.AppSettings["Countries"] + "delete/" + item.Id;

                //Формируем клиента, добавляем ему токен и тело запроса
                using HttpClient client = new();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ConfigurationManager.AppSettings["Token"]);

                //Получаем результат запроса
                HttpResponseMessage result = await client.DeleteAsync(url);

                //Если получили успешный результат
                if (result != null)
                {
                    if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        SetError("Некорректный токен", false);

                    //Десериализуем ответ
                    var content = await result.Content.ReadAsStringAsync();

                    BaseResponse response = JsonSerializer.Deserialize<BaseResponse>(content, _settings);

                    //Если успешно, оповещяем пользователя и обновляем страны
                    if (response.Success)
                    {
                        GetCountries(search, 0, skip + take, sort, isDeleted, true);
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

            //Разблокируем таблицу
            CountriesDataGrid.IsEnabled = true;

            //Обновляем страны
            GetCountries(search, 0, skip + take, sort, isDeleted, true);
        }
        catch (Exception ex)
        {
            _logger.Error("Countries. DeletedButton_Click. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Событие нажатия на кнопку восстановления стран
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void RestoredButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            //Получаем выбранную строку
            CountriesResponseListItem item = CountriesDataGrid.SelectedItem as CountriesResponseListItem;

            //Блокируем таблицу
            CountriesDataGrid.IsEnabled = false;

            //Проверяем ошибки
            if (item.Id == null)
            {
                SetError("Не указан id страны", false);
                return;
            }

            //Если в конфиге есть данные для формирования ссылки запроса
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["DefaultConnection"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Api"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Countries"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Token"]))
            {
                //Формируем ссылку запроса
                string url = ConfigurationManager.AppSettings["DefaultConnection"] + ConfigurationManager.AppSettings["Api"] +
                    ConfigurationManager.AppSettings["Countries"] + "restore/" + item.Id;

                //Формируем клиента, добавляем ему токен и тело запроса
                using HttpClient client = new();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ConfigurationManager.AppSettings["Token"]);

                //Получаем результат запроса
                HttpResponseMessage result = await client.DeleteAsync(url);

                //Если получили успешный результат
                if (result != null)
                {
                    if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        SetError("Некорректный токен", false);

                    //Десериализуем ответ
                    var content = await result.Content.ReadAsStringAsync();

                    BaseResponse response = JsonSerializer.Deserialize<BaseResponse>(content, _settings);

                    //Если успешно, оповещяем пользователя и обновляем страны
                    if (response.Success)
                    {
                        GetCountries(search, 0, skip + take, sort, isDeleted, true);
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

            //Разблокируем таблицу
            CountriesDataGrid.IsEnabled = true;

            //Обновляем страны
            GetCountries(search, 0, skip + take, sort, isDeleted, true);
        }
        catch (Exception ex)
        {
            _logger.Error("Countries. RestoredButton_Click. Ошибка: {0}", ex);
        }

    }
}
