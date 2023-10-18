using Client.Controls.InformationArticles;
using Client.Services.Base;
using Serilog;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Client.Controls;

/// <summary>
/// Логика взаимодействия для Base.xaml
/// </summary>
public partial class Base : UserControl
{
    public ILogger _logger { get { return Log.ForContext<Base>(); } } //логгер для записи логов
    public IBaseService _baseService; //базовый сервис
    List<string> _accessRights; //права доступа

    /// <summary>
    /// Конструктор базового окна
    /// </summary>
    public Base(IBaseService baseService, List<string> accessRights)
    {
        try
        {
            //Инициализируем компоненты
            InitializeComponent();

            //Записывае права доступа
            _accessRights = accessRights;

            //Если есть право доступа на страницу адимнистрирования
            if (_accessRights.Contains("Stranitsa_administrirovaniya"))
                //Отображаем кнопку администрирования
                AdministratorButton.Visibility = Visibility.Visible;

            //Формируем базовый сервис
            _baseService = baseService;

            //Формируем главную страницу и отображаем её
            Main main = new(_baseService);
            BaseContent.Content = main;
        }
        catch (Exception ex)
        {
            _logger.Error("Base. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Логика нажатия кнопки выхода
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OutButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            //Формируем новую страницу авторизации
            Authorization authorization = new(_baseService);

            //Убираем отступы
            Padding = new(0, 0, 0, 0);

            //Меняем основной контент на страницу авторизации
            Content = authorization;
        }
        catch (Exception ex)
        {
            _logger.Error("Base. OutButton_Click. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Логика нажатия кнопки администраторской части
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AdministratorButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            //Формируем новую страницуи администрирования
            Administrator administrator = new(_baseService);

            //Меняем контент на странице на страницу администрирования
            BaseContent.Content = administrator;
        }
        catch (Exception ex)
        {
            _logger.Error("Base. AdministratorButton_Click. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Логика нажатия кнопки возвращения на домашнюю страницу
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void HomeButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            //Формируем новую домашнюю страницу
            Main main = new(_baseService);

            //Меняем контент на странице на домашнюю страницу
            BaseContent.Content = main;
        }
        catch (Exception ex)
        {
            _logger.Error("Base. HomeButton_Click. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Логика нажатия кнопки отображения информационных статей
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void InformationArticleButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            //Формируем новую домашнюю страницу
            InformationArticleList informationArticleList = new();

            //Меняем контент на странице на домашнюю страницу
            BaseContent.Content = informationArticleList;
        }
        catch (Exception ex)
        {
            _logger.Error("Base. InformationArticleButton_Click. Ошибка: {0}", ex);
        }
    }
}
