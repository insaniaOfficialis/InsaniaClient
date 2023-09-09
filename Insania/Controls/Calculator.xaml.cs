using System.Windows;
using System.Windows.Controls;

namespace Insania.Controls
{
    public partial class Calculator : UserControl
    {
        public Calculator()
        {
            InitializeComponent();
        }

        private void OutButton_Click(object sender, RoutedEventArgs e)
        {
            Authorization authorization = new();

            this.Padding = new(0, 0, 0, 0);
            this.Content = authorization;
        }
    }
}
