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

namespace Client.Controls.Generators
{
    /// <summary>
    /// Логика взаимодействия для GeneratorResource.xaml
    /// </summary>
    public partial class GeneratorResource : UserControl
    {
        List<string> CountriesList = new();

        public GeneratorResource()
        {
            InitializeComponent();

            CountriesList.Add("Альвраатская империя");
            CountriesList.Add("Фесгарское княжество");
            CountriesList.Add("Альтерское княжество");
            CountriesList.Add("Орлиадарская конфеерация");

            Countries.ItemsSource = CountriesList;
        }
    }
}
