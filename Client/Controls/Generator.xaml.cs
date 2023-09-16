using Client.Controls.Generators;
using Serilog;
using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client.Controls;

/// <summary>
/// Логика взаимодействия для Generator.xaml
/// </summary>
public partial class Generator : UserControl
{
    public ILogger _logger { get { return Log.ForContext<Generator>(); } } //логгер для записи логов

    /// <summary>
    /// Конструктор страницы генераторов
    /// </summary>
    public Generator()
    {
        try
        {
            /*Инициализируем компоненты*/
            InitializeComponent();
        }
        catch (Exception ex)
        {
            _logger.Error("Generator. " + ex.Message);
        }
    }

    /// <summary>
    /// Событие нажатия на список
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TextBlock1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            /*Формируем новую страницу генератора ресурсов*/
            GeneratorResource generatorResource = new();

            /*Меняем контент элемента на генератор ресурсов*/
            Content.Content = generatorResource;
        }
        catch (Exception ex)
        {
            _logger.Error("Generator. TextBlock1_MouseLeftButtonDown. " + ex.Message);
        }
    }
}
