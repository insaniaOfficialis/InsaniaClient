using Client.Controls.Bases;
using Client.Services.Base;
using Domain.Models.Base;
using Domain.Models.General.Logs.Response;
using Queries.General.Logs.GetLogs;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.DirectoryServices;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client.Controls.Administrators;

/// <summary>
/// Логика взаимодействия для Logs.xaml
/// </summary>
public partial class Logs : UserControl
{
    private ILogger _logger { get { return Log.ForContext<Logs>(); } } //сервис для записи логов
    private IBaseService _baseService; //базовый сервис
    private IGetLogs _getLogs; //сервис получения логов
    private ObservableCollection<GetLogsResponseItem> _logs = new(); //коллекция логов
    private int? _skip = 0, _take = 20; //пагинация
    private string? _search; //строка поиска
    private bool _success = true; //признак успешности
    private DateTime? _from, _to;
    private List<BaseSortRequest>? _sort = new(); //список сортировки
    private LoadCircle _load = new(); //элемент загрузки

    /// <summary>
    /// Конструктор страницы логов
    /// </summary>
    public Logs(IBaseService baseService)
    {
        try
        {
            //Инициализируем компоненты
            InitializeComponent();

            //Получаем базовый сервис
            _baseService = baseService;

            //Проверяем доступность api
            _baseService.CheckConnection();

            //Формируем сервис получения логов
            _getLogs = new GetLogs();
        }
        catch (Exception ex)
        {
            _logger.Error("Logs. Ошибка: {0}", ex);
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
            _logger.Error("Logs. SetError. Ошибка: {0}", ex);
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
            LogsDataGrid.Items.SortDescriptions.Add(new SortDescription("Id", ListSortDirection.Descending));

            //Получаем логи
            var response = await _getLogs.Handler(_search, _skip, _take, _sort, _from, _to, _success);

            //Наполняем коллекцию логов
            if(response != null && response.Items.Any())
            {
                foreach(var item in response.Items)
                    _logs.Add(item);
            }

            LogsDataGrid.ItemsSource = _logs;
        }
        catch(Exception ex)
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
            var response = await _getLogs.Handler(_search, _skip, _take, _sort, _from, _to, _success);

            //Наполняем коллекцию логов
            if (response != null && response.Items.Any())
            {
                foreach (var item in response.Items)
                    _logs.Add(item);
            }

            LogsDataGrid.Items.Refresh();
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
            _logger.Error("Logs. TextBox_LostFocus. Ошибка: {0}", ex);
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
            _logger.Error("Logs. TextBoxEnter. Ошибка: {0}", ex);
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
                var response = await _getLogs.Handler(_search, _skip, _take, _sort, _from, _to, _success);

                //Очищаем и наполняем коллекцию логов
                if (response != null && response.Items.Any())
                {
                    _logs.Clear();

                    foreach (var item in response.Items)
                        _logs.Add(item);
                }

                //Обновляем таблицу
                LogsDataGrid.Items.Refresh();
            }
            else
            {
                SetError("Не указано значение для поиска", false);
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Logs. SearchButton_Click. Ошибка: {0}", ex);
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
    private async void SuccessRadioButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            //Включаем элемент загрузки
            LoadContent.Content = _load;
            LoadContent.Visibility = Visibility.Visible;

            //Устанавливаем параметры поиска
            _success = SuccessRadioButton.IsChecked ?? false;
            _skip = 0;
            _take = 20;

            //Получаем логи
            var response = await _getLogs.Handler(_search, _skip, _take, _sort, _from, _to, _success);

            //Очищаем и наполняем коллекцию логов
            if (response != null && response.Items.Any())
            {
                _logs.Clear();

                foreach (var item in response.Items)
                    _logs.Add(item);
            }

            //Обновляем таблицу
            LogsDataGrid.Items.Refresh();
        }
        catch (Exception ex)
        {
            _logger.Error("Logs. DeletedRadioButton_Checked. Ошибка: {0}", ex);
        }
        finally
        {
            //Отключаем элемент загрузки
            LoadContent.Content = null;
            LoadContent.Visibility = Visibility.Visible;
        }
    }

    /// <summary>
    /// Событие выбора даты от
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void FromDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            //Включаем элемент загрузки
            LoadContent.Content = _load;
            LoadContent.Visibility = Visibility.Visible;

            //Устанавливаем параметры поиска
            _from = FromDatePicker.SelectedDate;
            _skip = 0;
            _take = 20;

            //Получаем логи
            var response = await _getLogs.Handler(_search, _skip, _take, _sort, _from, _to, _success);

            //Очищаем и наполняем коллекцию логов
            if (response != null && response.Items.Any())
            {
                _logs.Clear();

                foreach (var item in response.Items)
                    _logs.Add(item);
            }

            //Обновляем таблицу
            LogsDataGrid.Items.Refresh();
        }
        catch (Exception ex)
        {
            _logger.Error("Logs. FromDatePicker_SelectedDateChanged. Ошибка: {0}", ex);
        }
        finally
        {
            //Отключаем элемент загрузки
            LoadContent.Content = null;
            LoadContent.Visibility = Visibility.Visible;
        }
    }

    /// <summary>
    /// Событие выбора даты до
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void ToDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            //Включаем элемент загрузки
            LoadContent.Content = _load;
            LoadContent.Visibility = Visibility.Visible;

            //Устанавливаем параметры поиска
            _to = ToDatePicker.SelectedDate;
            _skip = 0;
            _take = 20;

            //Получаем логи
            var response = await _getLogs.Handler(_search, _skip, _take, _sort, _from, _to, _success);

            //Очищаем и наполняем коллекцию логов
            if (response != null && response.Items.Any())
            {
                _logs.Clear();

                foreach (var item in response.Items)
                    _logs.Add(item);
            }

            //Обновляем таблицу
            LogsDataGrid.Items.Refresh();
        }
        catch (Exception ex)
        {
            _logger.Error("Logs. ToDatePicker_SelectedDateChanged. Ошибка: {0}", ex);
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
    private async void LogsDataGrid_Sorting(object sender, DataGridSortingEventArgs e)
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
                var response = await _getLogs.Handler(_search, _skip, _take, _sort, _from, _to, _success);

                //Очищаем и наполняем коллекцию логов
                if (response != null && response.Items.Any())
                {
                    _logs.Clear();

                    foreach (var item in response.Items)
                        _logs.Add(item);
                }

                //Обновляем таблицу
                LogsDataGrid.Items.Refresh();
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Logs. LogsDataGrid_Sorting. Ошибка: {0}", ex);
        }
        finally
        {
            //Отключаем элемент загрузки
            LoadContent.Content = null;
            LoadContent.Visibility = Visibility.Visible;
        }
    }
}