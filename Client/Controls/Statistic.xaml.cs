using Client.Controls.Administrators;
using Client.Controls.Statistics;
using Serilog;
using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client.Controls;

/// <summary>
/// Логика взаимодействия для Statistics.xaml
/// </summary>
public partial class Statistic : UserControl
{
    public ILogger _logger { get { return Log.ForContext<Administrator>(); } } //логгер для записи логов

    /// <summary>
    /// Конструктор страницы статистики
    /// </summary>
    public Statistic()
    {
        try
        {
            /*Инициализируем компоненты*/
            InitializeComponent();
        }
        catch (Exception ex)
        {
            _logger.Error("Statistic. Ошибка: {1}", ex);
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
            /*Определяем нажатый элемент как элемент списка*/
            var element = sender as ListBoxItem;

            /*Ищем наименование нажатого элемента*/
            switch (element.Name)
            {
                case "CountryItem":
                    {
                        /*Формируем страницу стран*/
                        Countries countries = new();

                        /*Меняем контент элемента на странице на страницу стран*/
                        Element.Content = countries;
                    }
                    break;
                case "RolesItem":
                    {
                        /*Формируем страницу регионов*/
                        Roles roles = new();

                        /*Меняем контент элемента на странице на страницу регионов*/
                        Element.Content = roles;
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Statistic. Element_MouseLeftButtonUp. Ошибка: {1}", ex);
        }
    }
}
