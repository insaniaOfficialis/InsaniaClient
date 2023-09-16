using Serilog;
using System;
using System.Windows.Controls;

namespace Client.Controls;

/// <summary>
/// Логика взаимодействия для Calculator.xaml
/// </summary>
public partial class Calculator : UserControl
{
    public ILogger _logger { get { return Log.ForContext<Calculator>(); } } //логгер для записи логов

    /// <summary>
    /// Конструктор страницы калькуляторов
    /// </summary>
    public Calculator()
    {
        try
        {
            /*Инициализируем компоненты*/
            InitializeComponent();
        }
        catch (Exception ex)
        {
            _logger.Error("Calculator. " + ex.Message);
        }
    }
}
