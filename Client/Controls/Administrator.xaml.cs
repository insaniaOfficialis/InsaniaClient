using Client.Controls.Administrators;
using Serilog;
using System;
using System.Windows.Controls;

namespace Client.Controls;

/// <summary>
/// Логика взаимодействия для Administrator.xaml
/// </summary>
public partial class Administrator : UserControl
{
    public ILogger _logger { get { return Log.ForContext<Administrator>(); } } //логгер для записи логов

    /// <summary>
    /// Конструктор страницы администраторской части
    /// </summary>
    public Administrator()
    {
        try
        {
            /*Инициализируем компоненты*/
            InitializeComponent();
        }
        catch (Exception ex)
        {
            _logger.Error("Administrator. " + ex.Message);
        }
    }

    /// <summary>
    /// Событие нажатия на элемент списка
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Element_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        try
        {
            /*Определяем нажатый элемент как элемент списка*/
            var element = sender as ListBoxItem;

            /*Ищем наименование нажатого элемента*/
            switch (element.Name)
            {
                case "RegistrationItem":
                    {
                        /*Формируем страницу регистрации*/
                        Registration registration = new();

                        /*Меняем контент элемента на странице на страницу регистрации*/
                        Element.Content = registration;
                    }
                    break;
                case "RolesItem":
                    {
                        /*Формируем страницу ролей*/
                        Roles roles = new();

                        /*Меняем контент элемента на странице на страницу ролей*/
                        Element.Content = roles;
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Administrator. Element_MouseLeftButtonUp. " + ex.Message);
        }
    }
}
