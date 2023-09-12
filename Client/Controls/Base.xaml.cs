using System.Windows;
using System.Windows.Controls;

namespace Client.Controls
{
    /// <summary>
    /// Логика взаимодействия для Base.xaml
    /// </summary>
    public partial class Base : UserControl
    {
        /// <summary>
        /// Конструктор базового окна
        /// </summary>
        public Base()
        {
            /*Инициализируем компоненты*/
            InitializeComponent();

            /*Заполняем контент из главного окна*/
            Main main = new();
            BaseContent.Content = main;
        }

        /// <summary>
        /// Логика кнопки выхода
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OutButton_Click(object sender, RoutedEventArgs e)
        {
            Authorization authorization = new();

            Padding = new(0, 0, 0, 0);
            Content = authorization;
        }

        /// <summary>
        /// Логика открытия окна администраторской части
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AdministratorButton_Click(object sender, RoutedEventArgs e)
        {
            Administrator administrator = new();
            BaseContent.Content = administrator;
        }
    }
}
