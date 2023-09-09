using Insania.Controls.Generators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Insania.Controls
{
    /// <summary>
    /// Логика взаимодействия для Generator.xaml
    /// </summary>
    public partial class Generator : UserControl
    {
        public Generator()
        {
            InitializeComponent();
        }

        private void OutButton_Click(object sender, RoutedEventArgs e)
        {
            Authorization authorization = new();

            this.Padding = new(0, 0, 0, 0);
            this.Content = authorization;
        }

        private void TextBlock1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            GeneratorResource generatorResource = new();
            Content.Content = generatorResource;
        }
    }
}
