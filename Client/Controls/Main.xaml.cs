using Domain.Models.MainWindow;
using Client.Services.Base;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    public IBaseService _baseService;

    /// <summary>
    /// Конструктор главной страницы
    /// </summary>
    public Main(IBaseService baseService)
    {
        try
        {
            //Инициализируем компоненты
            InitializeComponent();

            //Формируем новый базовый сервис
            _baseService = baseService;
        }
        catch(Exception ex)
        {
            _logger.Error("Main. Ошибка: {0}", ex);
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
            //Получаем элемент
            var element = sender as FrameworkElement;

            //Формируем новый конвертер цветов
            var bc = new BrushConverter();

            //Определяем наведенный элемент и в зависимости от него устанавливаем новый цвет
            switch (element.Name)
            {
                case "Label1": Polygon1.Fill = (Brush)bc.ConvertFrom("#696969"); break;
                case "Label2": Polygon2.Fill = (Brush)bc.ConvertFrom("#696969"); break;
                case "Label3": Polygon3.Fill = (Brush)bc.ConvertFrom("#696969"); break;
                case "Label4": Polygon4.Fill = (Brush)bc.ConvertFrom("#696969"); break;
                case "Label5": Polygon5.Fill = (Brush)bc.ConvertFrom("#696969"); break;
                case "StatisticsLabel": StatisticsPolygon.Fill = (Brush)bc.ConvertFrom("#696969"); break;
                default: { Polygon polygon = sender as Polygon; polygon.Fill = (Brush)bc.ConvertFrom("#696969"); break; }
            }
        }
        catch(Exception ex)
        {
            _logger.Error("Main. Polygon_MouseEnter. Ошибка: {0}", ex);
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
            //Получаем элемент
            var element = sender as FrameworkElement;

            //Формируем новый конвертер цветов
            var bc = new BrushConverter();

            //Определяем наведенный элемент и в зависимости от него устанавливаем новый цвет
            switch (element.Name)
            {
                case "Label1": Polygon1.Fill = (Brush)bc.ConvertFrom("#4D4D4D"); break;
                case "Label2": Polygon2.Fill = (Brush)bc.ConvertFrom("#4D4D4D"); break;
                case "Label3": Polygon3.Fill = (Brush)bc.ConvertFrom("#4D4D4D"); break;
                case "Label4": Polygon4.Fill = (Brush)bc.ConvertFrom("#4D4D4D"); break;
                case "Label5": Polygon5.Fill = (Brush)bc.ConvertFrom("#4D4D4D"); break;
                case "StatisticsLabel": StatisticsPolygon.Fill = (Brush)bc.ConvertFrom("#4D4D4D"); break;
                default: { Polygon polygon = sender as Polygon; polygon.Fill = (Brush)bc.ConvertFrom("#4D4D4D"); break; }
            }
        }
        catch(Exception ex)
        {
            _logger.Error("Main. Polygon_MouseLeave. Ошибка: {0}", ex);
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
            //Формируем новое окно калькулятора
            Calculator calculator = new();

            //Убираем отступы
            base.Padding = new(0, 0, 0, 0);

            //Заменяем контент
            base.Content = calculator;
        }
        catch (Exception ex)
        {
            _logger.Error("Main. Polygon4_MouseLeftButtonDown. Ошибка: {0}", ex);
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
            //Формируем новое окно калькулятора
            Generator generator = new(_baseService);

            //Убираем отступы
            base.Padding = new(0, 0, 0, 0);

            //Заменяем контент
            base.Content = generator;
        }
        catch (Exception ex)
        {
            _logger.Error("Main. Polygon5_MouseLeftButtonDown. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Метод нажатия на пункты меню
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Polygon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            //Убираем отступы
            base.Padding = new(0, 0, 0, 0);

            //Определяем нажатый элемент как полигон
            var elementPolygon = sender as Polygon;
            var elementLabel = sender as Label;

            if (elementPolygon != null)
            {
                //Ищем наименование нажатого элемента
                switch (elementPolygon.Name)
                {
                    case "StatisticsPolygon":
                        {
                            //Формируем страницу статистики
                            Statistic statistic = new();

                            //Меняем контент элемента на странице на страницу статистики
                            base.Content = statistic;
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                if (elementLabel != null)
                {
                    //Ищем наименование нажатого элемента
                    switch (elementLabel.Name)
                    {
                        case "StatisticsLabel":
                            {
                                //Формируем страницу статистики
                                Statistic statistic = new();

                                //Меняем контент элемента на странице на страницу статистики
                                base.Content = statistic;
                            }
                            break;
                        default:
                            break;
                    }
                }
                else
                    _logger.Error("Main. Polygon_MouseLeftButtonDown. Не удалось определить элемент");
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Main. Polygon_MouseLeftButtonDown. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Метод получения новостей
    /// </summary>
    private async Task GetNews()
    {
        try
        {
            //Формируем новый список новостей
            List<ShortNew> list = new();

            //Добавляем новостям источник в виде листа
            //News.ItemsSource = list;
        }
        catch (Exception ex)
        {
            _logger.Error("Main. GetNews. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Метод получения изображения пользователя
    /// </summary>
    private async Task GetLogo()
    {
        try
        {
            //Записываем путь изображения
            //LogoImage.Source = ImageSourceValueSerializer(@"I:\\Files\System\afe36d54-028a-4b44-934e-39841aac59bb\Alv.png");

            //Создаём анимацию
            DoubleAnimation animation = new()
            {
                From = 0.4,
                To = 0.6,
                AutoReverse = true,
                Duration = TimeSpan.FromSeconds(3),
                RepeatBehavior = RepeatBehavior.Forever,
            };

            //Запускаем анимацию
            LogoImage.BeginAnimation(UIElement.OpacityProperty, animation);
        }
        catch (Exception ex)
        {
            _logger.Error("Main. GetLogo. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Метод загрузки страницы
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    private async void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            //Получаем основные данные
            var connection = _baseService.CheckConnection();
            var news = GetNews();
            var logo = GetLogo();
            await Task.WhenAll(news, logo, connection);
        }
        catch (Exception ex)
        {
            _baseService.SetError(ex.Message);
        }
    }
}