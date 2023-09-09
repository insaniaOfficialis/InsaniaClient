using Client.Controls;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Client;

/// <summary>
/// Логика взаимодействия для MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    string connectionString;

    public MainWindow()
    {
        /*Инициализируем окно*/
        InitializeComponent();

        connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        /*Определяем действие для кнопки esc*/
        this.PreviewKeyDown += new KeyEventHandler(Close);
    }

    /// <summary>
    /// Закрытие окна
    /// </summary>
    public void Close(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
            Close();
    }

    /// <summary>
    /// Смена контента на авторизацию
    /// </summary>
    public async Task ShowAuthoriztion()
    {
        Authorization authorization = new Authorization();

        await Task.Delay(1000);

        this.Content = authorization;
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        /*Формируем стартовое окно*/
        Screensaver screensaver = new Screensaver();

        this.Content = screensaver;

        /*Создаём подключение*/
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            /*Открываем подключение*/
            await connection.OpenAsync();

            if (connection.State == ConnectionState.Open)
            {
                /*Отображаем страницу авторизации*/
                await ShowAuthoriztion();
            }
        }

    }
}
