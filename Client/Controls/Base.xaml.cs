using Serilog;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Client.Controls;

/// <summary>
/// Логика взаимодействия для Base.xaml
/// </summary>
public partial class Base : UserControl
{
    public ILogger _logger { get { return Log.ForContext<Base>(); } } //логгер для записи логов

    /// <summary>
    /// Конструктор базового окна
    /// </summary>
    public Base()
    {
        try
        {
            /*Инициализируем компоненты*/
            InitializeComponent();

            /*Заполняем контент из главного окна*/
            Main main = new();
            BaseContent.Content = main;
        }
        catch (Exception ex)
        {
            _logger.Error("Base. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Логика нажатия кнопки выхода
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OutButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            /*Формируем новую страницу авторизации*/
            Authorization authorization = new();

            /*Убираем отступы*/
            Padding = new(0, 0, 0, 0);

            /*Меняем основной контент на страницу авторизации*/
            Content = authorization;
        }
        catch (Exception ex)
        {
            _logger.Error("Base. OutButton_Click. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Логика нажатия кнопки администраторской части
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AdministratorButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            /*Формируем новую страницуи администрирования*/
            Administrator administrator = new();

            /*Меняем контент на странице на страницу администрирования*/
            BaseContent.Content = administrator;
        }
        catch (Exception ex)
        {
            _logger.Error("Base. AdministratorButton_Click. Ошибка: {0}", ex);
        }
    }
}
