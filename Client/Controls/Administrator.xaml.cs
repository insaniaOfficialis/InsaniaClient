using Client.Controls.Administrators;
using System.Windows;
using System.Windows.Controls;

namespace Client.Controls
{
    public partial class Administrator : UserControl
    {
        public Administrator()
        {
            InitializeComponent();
        }

        private void OutButton_Click(object sender, RoutedEventArgs e)
        {
            Authorization authorization = new();

            this.Padding = new(0, 0, 0, 0);
            this.Content = authorization;
        }

        private void Registration_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Registration registration = new();

            Element.Content = registration;
        }
    }
}
