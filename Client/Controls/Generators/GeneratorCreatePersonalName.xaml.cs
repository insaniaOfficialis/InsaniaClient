﻿using Serilog;
using System.Net.Http;
using System.Text.Json;
using System;
using System.Windows.Controls;
using System.Windows;
using System.Configuration;

namespace Client.Controls.Generators;

/// <summary>
/// Логика взаимодействия для GeneratorCreatePersonalName.xaml
/// </summary>
public partial class GeneratorCreatePersonalName : UserControl
{
    private ILogger _logger { get { return Log.ForContext<GeneratorCreatePersonalName>(); } } //сервис для записи логов
    private readonly JsonSerializerOptions _settings = new(); //настройки десериализации json

    /// <summary>
    /// Конструктор страницы генерации создания личных имён
    /// </summary>
    public GeneratorCreatePersonalName()
    {
        try
        {
            //Инициализация всех компонентов
            InitializeComponent();

            //Выставлыяем параметры десериализации
            _settings.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

            //Проверяем доступность api
            CheckConnection();
        }
        catch (Exception ex)
        {
            _logger.Error("GeneratorCreatePersonalName. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Метод проверки соединения с api
    /// </summary>
    public async void CheckConnection()
    {
        try
        {
            //Объявляем переменную ссылки запроса
            string url = null;

            try
            {
                //Если в конфиге есть данные для формирования ссылки запроса
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["DefaultConnection"]))
                {
                    //Формируем ссылку запроса
                    url = ConfigurationManager.AppSettings["DefaultConnection"];

                    //Получаем данные по запросу
                    using HttpClient client = new();

                    using var result = await client.GetAsync(url);

                    //Если получили успешный результат
                    if (result != null && result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        //Получаем начала

                        //Получение окончаний
                    }
                    //Иначе возвращаем ошибку
                    else
                        SetError("Сервер временно недоступен, попробуйте позднее или обратитесь в техническую поддержку", true);
                }
                //Иначе возвращаем ошибку
                else
                    SetError("Не указан адрес api. Обратитесь в техническую поддержку", true);
            }
            catch
            {
                SetError("Сервер временно недоступен, попробуйте позднее или обратитесь в техническую поддержку", true);
            }
        }
        catch (Exception ex)
        {
            SetError(ex.Message, true);
        }
    }

    /// <summary>
    /// Метод отображения ошибок
    /// </summary>
    /// <param name="message"></param>
    /// <param name="criticalException"></param>
    public void SetError(string message, bool criticalException)
    {
        try
        {
            //Объявляем переменные
            string style; //стиль

            //Определяем наименование стиля
            if (criticalException)
                style = "CriticalExceptionTextBlock";
            else
                style = "InnerExceptionTextBlock";

            //Находим стиль
            var exceptionStyle = FindResource(style) as Style;

            //Устанавливаем текст и стиль
            ErrorTextBlock.Style = exceptionStyle;
            ErrorTextBlock.Text = message;
        }
        catch (Exception ex)
        {
            _logger.Error("GeneratorCreatePersonalName. SetError. Ошибка: {0}", ex);
        }
    }
}