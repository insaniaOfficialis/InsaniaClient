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
    public ILogger _logger { get { return Log.ForContext<Generator>(); } } //сервис для записи логов

    /// <summary>
    /// Конструктор страницы администраторской части
    /// </summary>
    public Generator()
    {
        try
        {
            //Инициализируем компоненты
            InitializeComponent();
        }
        catch (Exception ex)
        {
            _logger.Error("Generator. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Событие нажатия на элемент списка
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Element_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        try
        {
            //Определяем нажатый элемент как элемент списка
            var element = sender as ListBoxItem;

            //Ищем наименование нажатого элемента
            switch (element.Name)
            {
                case "GeneratorResourceItem":
                    {
                        //Формируем страницу генерации ресурсов
                        GeneratorResource generatorResource = new();

                        //Меняем контент элемента на странице на страницу генерации ресурсов
                        Element.Content = generatorResource;
                    }
                    break;
                case "GeneratorPersonalNameItem":
                    {
                        //Формируем страницу генерации личных имён
                        GeneratorPersonalNames generatorPersonalNames = new();

                        //Меняем контент элемента на странице на страницу генерации личных имён
                        Element.Content = generatorPersonalNames;
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Generator. Element_MouseLeftButtonUp. Ошибка: {0}" + ex.Message);
        }
    }
}
