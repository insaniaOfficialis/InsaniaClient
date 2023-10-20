using Client.Controls.Administrators;
using Client.Services.Base;
using Serilog;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace Client.Controls;

/// <summary>
/// Логика взаимодействия для Administrator.xaml
/// </summary>
public partial class Administrator : UserControl
{
    public ILogger _logger { get { return Log.ForContext<Administrator>(); } } //логгер для записи логов
    public IBaseService _baseService; //базовый сервис
    List<string> _accessRights; //права доступа

    /// <summary>
    /// Конструктор страницы администраторской части
    /// </summary>
    public Administrator(IBaseService baseService, List<string> accessRights)
    {
        try
        {
            //Инициализируем компоненты
            InitializeComponent();

            //Получаем базовый сервис
            _baseService = baseService;

            //Проверяем доступность api
            _baseService.CheckConnection();

            //Получаем права доступа
            _accessRights = accessRights;

            //Если есть право доступа "Регистрация пользователей"
            if (accessRights.Contains("Registratsiya_pol'zovateley"))
                RegistrationItem.Visibility = Visibility.Visible;

            //Если есть право доступа "Создание ролей"
            if (accessRights.Contains("Sozdanie_roley"))
                RolesItem.Visibility = Visibility.Visible;

            //Если есть право доступа "Просмотр логов"
            if (accessRights.Contains("Prosmotr_logov"))
                LogsItem.Visibility = Visibility.Visible;

            //Если есть право доступа "Добавление имени"
            if (accessRights.Contains("Dobavlenie_imeni"))
                CreatePersonalNameItem.Visibility = Visibility.Visible;
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
                case "CreatePersonalNameItem":
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