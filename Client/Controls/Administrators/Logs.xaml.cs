using Client.Services.Base;
using Domain.Models.Base;
using Domain.Models.General.Logs.Response;
using Queries.General.Logs.GetLogs;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Client.Controls.Administrators;

/// <summary>
/// Логика взаимодействия для Logs.xaml
/// </summary>
public partial class Logs : UserControl
{
    private ILogger _logger { get { return Log.ForContext<Logs>(); } } //интиерфейс для записи логов
    private IBaseService _baseService; //базовый сервис
    private IGetLogs _getLogs; //сервис получения логов
    private ObservableCollection<GetLogsResponseItem> _logs = new(); //коллекция логов
    private int? _skip, _take = 20; //пагинация
    private string? _search; //строка поиска
    private bool _success = true; //признак успешности
    private DateTime? _from, _to;
    private List<BaseSortRequest>? _sort = new(); //список сортировки

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
    private async void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        try
        {
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

    }
}
