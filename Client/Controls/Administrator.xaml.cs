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

        private void Registration_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Registration registration = new();

            Element.Content = registration;
        }
    }
}
