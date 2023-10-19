using Client.Services.Base;
using Serilog;
using System;
using System.Windows.Controls;

namespace Client.Controls.Administrators;

/// <summary>
/// Логика взаимодействия для Logs.xaml
/// </summary>
public partial class Logs : UserControl
{
    public ILogger _logger { get { return Log.ForContext<Logs>(); } } //интиерфейс для записи логов
    public IBaseService _baseService; //базовый сервис

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
        }
        catch (Exception ex)
        {
            _logger.Error("Logs. Ошибка: {0}", ex);
        }
    }
}
