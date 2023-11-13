using Client.Controls.Bases;
using Client.Services.Base;
using Domain.Models.Base;
using Domain.Models.Identification.Users.Internal;
using Domain.Models.Informations.News.Response;
using Queries.Informations.News.GetNewsTable;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client.Controls.Administrators;

/// <summary>
/// Логика взаимодействия для NewsManagment.xaml
/// </summary>
public partial class NewsManagment : UserControl
{
    private ILogger _logger { get { return Log.ForContext<NewsManagment>(); } } //сервис для записи логов
    private IBaseService _baseService; //базовый сервис
    private IGetNewsTable _getNewsTable; //сервис получения списка новостей для таблицы
    private ObservableCollection<GetNewsTableResponseItem> _news = new(); //коллекция новостей
    private int? _skip = 0, _take = 20; //пагинация
    private string? _search; //строка поиска
    private bool _isDeleted = false; //признак увдалённой
    private List<BaseSortRequest>? _sort = new(); //список сортировки
    private LoadCircle _load = new(); //элемент загрузки
    //private List<string> _accessRights; //права достуа
    private AccessRightAction _accessRightAction = new();

    /// <summary>
    /// Конструктор страницы управления новостями
    /// </summary>
    /// <param name="baseService"></param>
    /// <param name="accessRights"></param>
    public NewsManagment(IBaseService baseService, List<string> accessRights)
    {
        try
        {
            //Инициализируем компоненты
            InitializeComponent();

            //Получаем базовый сервис
            _baseService = baseService;

            //Проверяем доступность api
            _baseService.CheckConnection();

            //Формируем сервис получения списка новостей для таблицы
            _getNewsTable = new GetNewsTable();

            //Если есть право доступа "Добавление новости"
            if (accessRights.Contains("Dobavlenie_novosti"))
                AddButton.Visibility = Visibility.Visible;

            //Если есть право доступа "Редактирование новости"
            if (accessRights.Contains("Redaktirovanie_novosti"))
                _accessRightAction.Edit = true;

            //Если есть право доступа "Удаление новости"
            if (accessRights.Contains("Udalenie_novosti"))
                _accessRightAction.Delete = true;

            //Если есть право доступа "Восстановление новости"
            if (accessRights.Contains("Vosstanovlenie_novosti"))
                _accessRightAction.Restore = true;

        }
        catch (Exception ex)
        {
            _logger.Error("News. Ошибка: {0}", ex);
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
            _logger.Error("News. SetError. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Событие загрузки окна
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            //Включаем элемент загрузки
            LoadContent.Content = _load;
            LoadContent.Visibility = Visibility.Visible;

            //Ставим сортировки по умолчанию
            _sort.Add(new("id", false));
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
                PaginationButton.Visibility = Visibility.Collapsed;
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

    /// <summary>
    /// Событие нажатия кнопки пагинации
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void PaginationButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            //Включаем элемент загрузки
            LoadContent.Content = _load;
            LoadContent.Visibility = Visibility.Visible;

            //Устанавливаем новое количество пропускаемых элементов
            _skip += _take;

            //Получаме новости
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
            NewsDataGrid.Items.Refresh();

            //Обрабываем видимость кнопки пагинации
            if (_news.Count > 19)
                PaginationButton.Visibility = Visibility.Visible;
            else
                PaginationButton.Visibility = Visibility.Collapsed;
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
            _logger.Error("News. TextBox_LostFocus. Ошибка: {0}", ex);
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
            _logger.Error("News. TextBoxEnter. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Метод нажатия на поиск
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            //Включаем элемент загрузки
            LoadContent.Content = _load;
            LoadContent.Visibility = Visibility.Visible;

            //Если есть введённый текст, кроме дефолтного
            if (!String.IsNullOrEmpty(SearchTextBox.Text) && SearchTextBox.Text != "Поиск...")
            {
                //Устанавливаем параметры поиска
                _search = SearchTextBox.Text;
                _skip = 0;
                _take = 20;

                //Получаме новости
                var response = await _getNewsTable.Handler(_search, _skip, _take, _sort, _isDeleted);

                //Очищаем коллекцию новостей
                _news.Clear();

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
                NewsDataGrid.Items.Refresh();

                //Обрабываем видимость кнопки пагинации
                if (_news.Count > 19)
                    PaginationButton.Visibility = Visibility.Visible;
                else
                    PaginationButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                SetError("Не указано значение для поиска", false);
            }
        }
        catch (Exception ex)
        {
            _logger.Error("News. SearchButton_Click. Ошибка: {0}", ex);
        }
        finally
        {
            //Отключаем элемент загрузки
            LoadContent.Content = null;
            LoadContent.Visibility = Visibility.Visible;
        }
    }

    /// <summary>
    /// Событие нажатия на переключатель успешности
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void DeletedRadioButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            //Включаем элемент загрузки
            LoadContent.Content = _load;
            LoadContent.Visibility = Visibility.Visible;

            //Устанавливаем параметры поиска
            _isDeleted = DeletedRadioButton.IsChecked ?? false;
            _skip = 0;
            _take = 20;

            //Получаме новости
            var response = await _getNewsTable.Handler(_search, _skip, _take, _sort, _isDeleted);
            
            //Очищаем коллекцию новостей
            _news.Clear();

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

            //Меняем действие
            if (_isDeleted)
            {
                Deleted.Visibility = Visibility.Collapsed;
                Restored.Visibility = Visibility.Visible;
            }
            else
            {
                Deleted.Visibility = Visibility.Visible;
                Restored.Visibility = Visibility.Collapsed;
            }

            //Обновляем таблицу
            NewsDataGrid.Items.Refresh();

            //Обрабываем видимость кнопки пагинации
            if (_news.Count > 19)
                PaginationButton.Visibility = Visibility.Visible;
            else
                PaginationButton.Visibility = Visibility.Collapsed;
        }
        catch (Exception ex)
        {
            _logger.Error("News. DeletedRadioButton_Checked. Ошибка: {0}", ex);
        }
        finally
        {
            //Отключаем элемент загрузки
            LoadContent.Content = null;
            LoadContent.Visibility = Visibility.Visible;
        }
    }

    /// <summary>
    /// Событие сортировки таблицы
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void NewsDataGrid_Sorting(object sender, DataGridSortingEventArgs e)
    {
        try
        {
            //Включаем элемент загрузки
            LoadContent.Content = _load;
            LoadContent.Visibility = Visibility.Visible;

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
                    _sort.Clear();
                }

                //Если уже есть такое поле сортировки
                if (_sort.Any(x => x.SortKey == sortKey))
                {
                    //Если порядок сортировки исчезает
                    if (isAscending == null)
                        //Убираем из полей сортировки
                        _sort.RemoveAll(x => x.SortKey == sortKey);
                    //Иначе
                    else
                        //Меняем порядлок сортировки
                        _sort.Where(x => x.SortKey == sortKey && x.IsAscending != isAscending)
                            .ToList()
                            .ForEach(x => x.IsAscending = isAscending);
                }
                //Иначе
                else
                {
                    //Если есть порядок сортировки
                    if (isAscending != null)
                        //Добавляем в сортировку новое поле
                        _sort.Add(new(sortKey, isAscending ?? false));
                }

                //Обнуляем пагинацию
                _skip = 0;
                _take = 20;

                //Получаме новости
                var response = await _getNewsTable.Handler(_search, _skip, _take, _sort, _isDeleted);

                //Очищаем коллекцию новостей
                _news.Clear();

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
                NewsDataGrid.Items.Refresh();

                //Обрабываем видимость кнопки пагинации
                if (_news.Count > 19)
                    PaginationButton.Visibility = Visibility.Visible;
                else
                    PaginationButton.Visibility = Visibility.Collapsed;
            }
        }
        catch (Exception ex)
        {
            _logger.Error("News. NewsDataGrid_Sorting. Ошибка: {0}", ex);
        }
        finally
        {
            //Отключаем элемент загрузки
            LoadContent.Content = null;
            LoadContent.Visibility = Visibility.Visible;
        }
    }

    /// <summary>
    /// События нажатия на кнопку добавления стран
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AddButton_Click(object sender, RoutedEventArgs e)
    {/*
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
        }*/
    }

    /// <summary>
    /// Событие нажатия на кнопку редактирвоания страны
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void EditButton_Click(object sender, RoutedEventArgs e)
    {/*
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
        }*/
    }

    /// <summary>
    /// Событие нажатия на кнопку удаления стран
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void DeletedButton_Click(object sender, RoutedEventArgs e)
    {/*
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
        }*/
    }

    /// <summary>
    /// Событие нажатия на кнопку восстановления стран
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void RestoredButton_Click(object sender, RoutedEventArgs e)
    {/*
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
        }*/
    }
}