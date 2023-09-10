using Client.Models.MainWindow;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Client.Controls
{
    /// <summary>
    /// Логика взаимодействия для Main.xaml
    /// </summary>
    public partial class Main : UserControl
    {
        string connectionString;

        public Main()
        {
            InitializeComponent();

            connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            GetNews();

            DoubleAnimation animation = new()
            {
                From = 0.4,
                To = 0.6,
                AutoReverse = true,
                Duration = TimeSpan.FromSeconds(3),
                RepeatBehavior = RepeatBehavior.Forever,
            };
            LogoImage.BeginAnimation(UIElement.OpacityProperty, animation);
        }

        private void Polygon_MouseEnter(object sender, MouseEventArgs e)
        {
            var element = sender as FrameworkElement;

            var bc = new BrushConverter();

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

        private void Polygon_MouseLeave(object sender, MouseEventArgs e)
        {
            var element = sender as FrameworkElement;

            var bc = new BrushConverter();

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

        private async void GetNews()
        {
            List<ShortNew> list = new();

            /*Создаём подключение*/
            using (SqlConnection connection = new(connectionString))
            {
                /*Открываем подключение*/
                await connection.OpenAsync();

                if (connection.State == ConnectionState.Open)
                {
                    string sqlExpression = "SELECT * FROM rNews WHERE isShow = 1 ORDER BY dateCreate DESC";

                    SqlCommand command = new(sqlExpression, connection);

                    /*Выполняем команду*/
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
            }

            //News.ItemsSource = list;
        }

        private void OutButton_Click(object sender, RoutedEventArgs e)
        {
            Authorization authorization = new();

            this.Padding = new(0,0,0,0);
            this.Content = authorization;
        }

        private void Polygon4_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Calculator calculator = new();
            this.Padding = new(0, 0, 0, 0);
            this.Content = calculator;
        }
        private void Polygon5_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Generator generator = new();
            this.Padding = new(0, 0, 0, 0);
            this.Content = generator;
        }

        private async void GetLogo()
        {
            //LogoImage.Source = ImageSourceValueSerializer(@"I:\\Files\System\afe36d54-028a-4b44-934e-39841aac59bb\Alv.png");
        }

        private void AdministratorButton_Click(object sender, RoutedEventArgs e)
        {
            Administrator administrator = new();
            this.Padding = new(0, 0, 0, 0);
            this.Content = administrator;
        }
    }
}
