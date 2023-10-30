using Client.Controls;
using Domain.Models.Identification.Users.Response;
using Client.Services.Base;
using Serilog;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Queries.General.CheckConnection.CheckAuthorize;
using System.Collections.Generic;

namespace Client;

/// <summary>
/// Логика взаимодействия для MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public ILogger _logger { get { return Log.ForContext<MainWindow>(); } } //логгер для записи логов
    public IBaseService _baseService; //базовый сервис
    private ICheckAuthorize _checkAuthorize; //сервис проверки соединения

    /// <summary>
    /// Конструктор главного окна
    /// </summary>
    public MainWindow()
    {
        try
        {
            //Инициализируем окно
            InitializeComponent();

            //Формируем базовый сервис
            _baseService = new BaseService();

            //Формируем сервис проверки соединения
            _checkAuthorize = new CheckAuthorize();

            //Определяем действие для кнопки esc
            PreviewKeyDown += new KeyEventHandler(Close);
        }
        catch(Exception ex)
        {
            _logger.Error("MainWindow. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Закрытие окна
    /// </summary>
    public void Close(object sender, KeyEventArgs e)
    {
        try
        {
            //Если нажата клавиша escape
            if (e.Key == Key.Escape)
                //Закрываем окно
                Close();
        }
        catch (Exception ex)
        {
            _logger.Error("MainWindow. Close. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Смена контента на авторизацию
    /// </summary>
    public async Task ShowAuthoriztion()
    {
        try
        {
            //Формируем окно авторизации
            Authorization authorization = new(_baseService);

            //Делаем паузу
            var stop = Task.Delay(1000);

            await Task.WhenAll(stop);

            //Меняем контент
            Content = authorization;
        }
        catch (Exception ex)
        {
            _logger.Error("MainWindow. ShowAuthoriztion. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Смена контента на основную страницу
    /// </summary>
    public async Task ShowBase()
    {
        try
        {
            //Получаем информацию о пользователе
            var userInfo = await _baseService.GetUserInfo();

            //Формируем окно авторизации
            Base baseWindow = new(_baseService, userInfo.AccessRights);

            //Меняем контент
            Content = baseWindow;
        }
        catch (Exception ex)
        {
            _logger.Error("MainWindow. ShowAuthoriztion. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Событие загрузки экрана
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            //Формируем стартовое окно
            Screensaver screensaver = new Screensaver();

            //Меняем контент
            Content = screensaver;

            //Проверяем соединение
            bool validateToken = false;
            try
            {
                if (await _checkAuthorize.Handler())
                    validateToken = true;
            }
            catch(Exception ex)
            {
                validateToken = false;
            }

            //Если проверка соединения пройдена
            if (validateToken)
                await ShowBase();
            //Иначе отображаем страницу авторизации
            else
                await ShowAuthoriztion();
        }
        catch (Exception ex)
        {
            _logger.Error("MainWindow. Window_Loaded. Ошибка: {0}", ex);
        }
    }
}