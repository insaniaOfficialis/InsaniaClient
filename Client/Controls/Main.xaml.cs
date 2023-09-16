using Client.Models.MainWindow;
using Serilog;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Client.Controls;

/// <summary>
/// Логика взаимодействия для Main.xaml
/// </summary>
public partial class Main : UserControl
{
    public ILogger _logger { get { return Log.ForContext<Main>(); } } //логгер для записи логов

    /// <summary>
    /// Конструктор главной страницы
    /// </summary>
    public Main()
    {
        try
        {
            /*Инициализируем компоненты*/
            InitializeComponent();

            /*Получаем новости*/
            GetNews();

            /*Получаем изображение*/
            GetLogo();
        }
        catch(Exception ex)
        {
            _logger.Error("Main. " + ex.Message);
        }
    }

    /// <summary>
    /// Событие наведения на пункты меню
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Polygon_MouseEnter(object sender, MouseEventArgs e)
    {
        try
        {
            /*Получаем элемент*/
            var element = sender as FrameworkElement;

            /*Формируем новый конвертер цветов*/
            var bc = new BrushConverter();

            /*Определяем наведенный элемент и в зависимости от него устанавливаем новый цвет*/
            switch (element.Name)
            {
                case "Label1": Polygon1.Fill = (Brush)bc.ConvertFrom("#696969"); break;
                case "Label2": Polygon2.Fill = (Brush)bc.ConvertFrom("#696969"); break;
                case "Label3": Polygon3.Fill = (Brush)bc.ConvertFrom("#696969"); break;
                case "Label4": Polygon4.Fill = (Brush)bc.ConvertFrom("#696969"); break;
                case "Label5": Polygon5.Fill = (Brush)bc.ConvertFrom("#696969"); break;
                case "Label6": Polygon6.Fill = (Brush)bc.ConvertFrom("#696969"); break;
                default: { Polygon polygon = sender as Polygon; polygon.Fill = (Brush)bc.ConvertFrom("#696969"); break; }
            }
        }
        catch(Exception ex)
        {
            _logger.Error("Main. Polygon_MouseEnter. " + ex.Message);
        }
    }

    /// <summary>
    /// Событие потери наведения на пункты меню
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Polygon_MouseLeave(object sender, MouseEventArgs e)
    {
        try
        {
            /*Получаем элемент*/
            var element = sender as FrameworkElement;

            /*Формируем новый конвертер цветов*/
            var bc = new BrushConverter();

            /*Определяем наведенный элемент и в зависимости от него устанавливаем новый цвет*/
            switch (element.Name)
            {
                case "Label1": Polygon1.Fill = (Brush)bc.ConvertFrom("#4D4D4D"); break;
                case "Label2": Polygon2.Fill = (Brush)bc.ConvertFrom("#4D4D4D"); break;
                case "Label3": Polygon3.Fill = (Brush)bc.ConvertFrom("#4D4D4D"); break;
                case "Label4": Polygon4.Fill = (Brush)bc.ConvertFrom("#4D4D4D"); break;
                case "Label5": Polygon5.Fill = (Brush)bc.ConvertFrom("#4D4D4D"); break;
                case "Label6": Polygon6.Fill = (Brush)bc.ConvertFrom("#4D4D4D"); break;
                default: { Polygon polygon = sender as Polygon; polygon.Fill = (Brush)bc.ConvertFrom("#4D4D4D"); break; }
            }
        }
        catch(Exception ex)
        {
            _logger.Error("Main. Polygon_MouseLeave. " + ex.Message);
        }
    }

    /// <summary>
    /// Метод получения новостей
    /// </summary>
    private void GetNews()
    {
        try
        {
            /*Формируем новый список новостей*/
            List<ShortNew> list = new();

            /*Создаём подключение*/
            /*using (SqlConnection connection = new(connectionString))
            {
                await connection.OpenAsync();

                if (connection.State == ConnectionState.Open)
                {
                    string sqlExpression = "SELECT * FROM rNews WHERE isShow = 1 ORDER BY dateCreate DESC";

                    SqlCommand command = new(sqlExpression, connection);
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            ShortNew shortNew = new()
                            {
                                Id = (Guid)reader["id"],
                                Title = (string)reader["title"],
                                ShortBody = (string)reader["shortBody"]
                            };

                            list.Add(shortNew);
                        }
                    }
                }
            }*/

            /*Добавляем новостям источник в виде листа*/
            //News.ItemsSource = list;
        }
        catch (Exception ex)
        {
            _logger.Error("Main. GetNews. " + ex.Message);
        }
    }

    /// <summary>
    /// События нажатия на пункт меню "Калькуляторы"
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Polygon4_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            /*Формируем новое окно калькулятора*/
            Calculator calculator = new();

            /*Убираем отступы*/
            Padding = new(0, 0, 0, 0);

            /*Заменяем контент*/
            Content = calculator;
        }
        catch (Exception ex)
        {
            _logger.Error("Main. Polygon4_MouseLeftButtonDown. " + ex.Message);
        }
    }

    /// <summary>
    /// События нажатия на пункт меню "Генераторы"
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Polygon5_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            /*Формируем новое окно калькулятора*/
            Generator generator = new();

            /*Убираем отступы*/
            Padding = new(0, 0, 0, 0);

            /*Заменяем контент*/
            Content = generator;
        }
        catch (Exception ex)
        {
            _logger.Error("Main. Polygon5_MouseLeftButtonDown. " + ex.Message);
        }
    }

    /// <summary>
    /// Метод получения изображения пользователя
    /// </summary>
    private void GetLogo()
    {
        try
        {
            /*Записываем путь изображения*/
            //LogoImage.Source = ImageSourceValueSerializer(@"I:\\Files\System\afe36d54-028a-4b44-934e-39841aac59bb\Alv.png");

            /*Создаём анимацию*/
            DoubleAnimation animation = new()
            {
                From = 0.4,
                To = 0.6,
                AutoReverse = true,
                Duration = TimeSpan.FromSeconds(3),
                RepeatBehavior = RepeatBehavior.Forever,
            };

            /*Запускаем анимацию*/
            LogoImage.BeginAnimation(UIElement.OpacityProperty, animation);
        }
        catch (Exception ex)
        {
            _logger.Error("Main. GetLogo. " + ex.Message);
        }
    }
}
