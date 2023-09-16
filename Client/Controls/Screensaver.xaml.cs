using Serilog;
using System;
using System.Windows.Controls;

namespace Client.Controls;

/// <summary>
/// Логика взаимодействия для Screensaver.xaml
/// </summary>
public partial class Screensaver : UserControl
{
    public ILogger _logger { get { return Log.ForContext<Screensaver>(); } } //логгер для записи логов

    /// <summary>
    /// Конструктор окна загрузки
    /// </summary>
    public Screensaver()
    {
        try
        {
            /*Инициализируем компоненты*/
            InitializeComponent();
        }
        catch(Exception ex)
        {
            _logger.Error("Screensaver. " + ex.Message);
        }
    }
}
