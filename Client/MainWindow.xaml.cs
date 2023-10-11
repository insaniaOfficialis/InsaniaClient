using Client.Controls;
using Serilog;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Client;

/// <summary>
/// Логика взаимодействия для MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public ILogger _logger { get { return Log.ForContext<MainWindow>(); } } //логгер для записи логов

    /// <summary>
    /// Конструктор главного окна
    /// </summary>
    public MainWindow()
    {
        try
        {
            /*Инициализируем окно*/
            InitializeComponent();

            /*Определяем действие для кнопки esc*/
            PreviewKeyDown += new KeyEventHandler(Close);
        }
        catch(Exception ex)
        {
            _logger.Error("MainWindow. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Закрытие окна
    /// </summary>
    public void Close(object sender, KeyEventArgs e)
    {
        try
        {
            /*Если нажата клавиша escape*/
            if (e.Key == Key.Escape)
                /*Закрываем окно*/
                Close();
        }
        catch (Exception ex)
        {
            _logger.Error("MainWindow. Close. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Смена контента на авторизацию
    /// </summary>
    public async Task ShowAuthoriztion()
    {
        try
        {
            /*Формируем окно авторизации*/
            Authorization authorization = new();

            /*Делаем паузу*/
            await Task.Delay(1000);

            /*Меняем контент*/
            Content = authorization;
        }
        catch (Exception ex)
        {
            _logger.Error("MainWindow. ShowAuthoriztion. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Событие загрузки экрана
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            /*Формируем стартовое окно*/
            Screensaver screensaver = new Screensaver();

            /*Меняем контент*/
            Content = screensaver;

            /*Отображаем страницу авторизации*/
            await ShowAuthoriztion();
        }
        catch (Exception ex)
        {
            _logger.Error("MainWindow. Window_Loaded. Ошибка: {0}", ex);
        }
    }
}
