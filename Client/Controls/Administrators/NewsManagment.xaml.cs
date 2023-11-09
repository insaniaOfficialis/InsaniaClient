using Client.Controls.Bases;
using Client.Services.Base;
using Domain.Models.Base;
using Domain.Models.Informations.News.Response;
using Queries.Informations.News.GetNewsTable;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
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

    /// <summary>
    /// Конструктор страницы управления новостями
    /// </summary>
    public NewsManagment(IBaseService baseService)
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

            //Получаем логи
            var response = await _getNewsTable.Handler(_search, _skip, _take, _sort, _isDeleted);

            //Наполняем коллекцию логов
            if (response != null && response.Items.Any())
            {
                foreach (var item in response.Items)
                    _news.Add(item);
            }

            NewsDataGrid.ItemsSource = _news;
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

            //Получаем логи
            var response = await _getNewsTable.Handler(_search, _skip, _take, _sort, _isDeleted);

            //Наполняем коллекцию логов
            if (response != null && response.Items.Any())
            {
                foreach (var item in response.Items)
                    _news.Add(item);
            }

            NewsDataGrid.Items.Refresh();
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

                //Получаем логи
                var response = await _getNewsTable.Handler(_search, _skip, _take, _sort, _isDeleted);

                //Очищаем и наполняем коллекцию логов
                if (response != null && response.Items.Any())
                {
                    _news.Clear();

                    foreach (var item in response.Items)
                        _news.Add(item);
                }

                //Обновляем таблицу
                NewsDataGrid.Items.Refresh();
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

            //Получаем логи
            var response = await _getNewsTable.Handler(_search, _skip, _take, _sort, _isDeleted);

            //Очищаем и наполняем коллекцию логов
            if (response != null && response.Items.Any())
            {
                _news.Clear();

                foreach (var item in response.Items)
                    _news.Add(item);
            }

            //Обновляем таблицу
            NewsDataGrid.Items.Refresh();
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

                //Получаем логи
                var response = await _getNewsTable.Handler(_search, _skip, _take, _sort, _isDeleted);

                //Очищаем и наполняем коллекцию логов
                if (response != null && response.Items.Any())
                {
                    _news.Clear();

                    foreach (var item in response.Items)
                        _news.Add(item);
                }

                //Обновляем таблицу
                NewsDataGrid.Items.Refresh();
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
}