using Client.Controls.Bases;
using Client.Controls.Generators;
using Domain.Models.Informations.InformationArticlesDetails.Response;
using Queries.Informations.InformationArticles.GetInformationArticleDetails;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace Client.Controls.InformationArticles;

/// <summary>
/// Логика взаимодействия для InformationArticle.xaml
/// </summary>
public partial class InformationArticle : UserControl
{
    private ILogger _logger { get { return Log.ForContext<GeneratorCreatePersonalName>(); } } //сервис для записи логов
    private readonly JsonSerializerOptions _settings = new(); //настройки десериализации json
    private IGetInformationArticleDetails _getInformationArticleDetails; //сервис получения списка детальных частей информационной статьи
    private LoadCircle _load = new(); //элемент загрузки
    private long? _informationArticleId; //ссылка на информационную статью
    private LinkedList<GetInformationArticleDetailsResponseItem> _details; //список детальных частей

    /// <summary>
    /// Конструктор страницы информациионных статей
    /// </summary>
    public InformationArticle(long? informationArticleId)
    {
        try
        {
            //Инициализация всех компонентов
            InitializeComponent();

            //Выставляем параметры десериализации
            _settings.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

            //Присваиваем ссылку на информационную статью
            _informationArticleId = informationArticleId;

            //Формируем экземпляр сервиса получения детальных частей информационной статьи
            _getInformationArticleDetails = new GetInformationArticleDetails();

            //Проверяем доступность api
            CheckConnection();
        }
        catch (Exception ex)
        {
            _logger.Error("InformationArticle. Ошибка: {0}", ex);
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
            _logger.Error("InformationArticle. SetError. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Событие загрузки окна
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            //Включаем элемент загрузки
            Element.Content = _load;
            Element.Visibility = Visibility.Visible;

            //Получаем информационные статьи
            var response = await _getInformationArticleDetails.Handler(_informationArticleId);

            //Наполняем коллекцию логов
            if (response != null && response.Items.Any())
            {
                _details = new(response.Items);

                //Присваиваем первый текст
                Text.Text = _details.First().Text;
            }
        }
        catch (Exception ex)
        {
            SetError(ex.Message, true);
        }
        finally
        {
            //Отключаем элемент загрузки
            Element.Content = null;
            Element.Visibility = Visibility.Visible;
        }
    }
}