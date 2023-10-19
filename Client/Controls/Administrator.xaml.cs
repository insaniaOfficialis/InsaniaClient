using Client.Controls.Administrators;
using Client.Services.Base;
using Serilog;
using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client.Controls;

/// <summary>
/// Логика взаимодействия для Administrator.xaml
/// </summary>
public partial class Administrator : UserControl
{
    public ILogger _logger { get { return Log.ForContext<Administrator>(); } } //логгер для записи логов
    public IBaseService _baseService; //базовый сервис

    /// <summary>
    /// Конструктор страницы администраторской части
    /// </summary>
    public Administrator(IBaseService baseService)
    {
        try
        {
            //Инициализируем компоненты
            InitializeComponent();

            //Получаем базовый сервис
            _baseService = baseService;

            //Проверяем доступность api
            _baseService.CheckConnection();
        }
        catch (Exception ex)
        {
            _logger.Error("Administrator. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Событие нажатия на элемент списка
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Element_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        try
        {
            //Определяем нажатый элемент как элемент списка
            var element = sender as ListBoxItem;

            //Ищем наименование нажатого элемента
            switch (element.Name)
            {
                case "RegistrationItem":
                    {
                        //Формируем страницу регистрации
                        Registration registration = new();

                        //Меняем контент элемента на странице на страницу регистрации
                        Element.Content = registration;
                    }
                    break;
                case "RolesItem":
                    {
                        //Формируем страницу ролей
                        Roles roles = new();

                        //Меняем контент элемента на странице на страницу ролей
                        Element.Content = roles;
                    }
                    break;
                case "CreatePersonalName":
                    {
                        //Формируем страницу создания имени
                        CreatePersonalName page = new(_baseService);

                        //Меняем контент элемента на странице на страницу создания имени
                        Element.Content = page;
                    }
                    break;
                case "LogsItem":
                    {
                        //Формируем страницу логов
                        Logs page = new(_baseService);

                        //Меняем контент элемента на странице на страницу логов
                        Element.Content = page;
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Administrator. Element_MouseLeftButtonUp. Ошибка: {0}", ex);
        }
    }
}