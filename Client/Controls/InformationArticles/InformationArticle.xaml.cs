using Client.Controls.Bases;
using Client.Controls.Generators;
using Domain.Models.Informations.InformationArticlesDetails.Response;
using Queries.General.Files.GetFileUrl;
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
using System.Windows.Media.Imaging;

namespace Client.Controls.InformationArticles;

/// <summary>
/// Логика взаимодействия для InformationArticle.xaml
/// </summary>
public partial class InformationArticle : UserControl
{
    private ILogger _logger { get { return Log.ForContext<GeneratorCreatePersonalName>(); } } //сервис для записи логов
    private readonly JsonSerializerOptions _settings = new(); //настройки десериализации json
    private IGetInformationArticleDetails _getInformationArticleDetails; //сервис получения списка детальных частей информационной статьи
    private IGetFileUrl _getFileUrl; //сервис получения ссылки файла
    private LoadCircle _load = new(); //элемент загрузки
    private long? _informationArticleId; //ссылка на информационную статью
    private LinkedList<GetInformationArticleDetailsResponseItem> _details; //список детальных частей
    private LinkedListNode<GetInformationArticleDetailsResponseItem> _currentDetail; //текущая детальная часть
    private LinkedList<string> _files; //список файлов детальной части 
    private LinkedListNode<string> _currentFile; //текущий файл детальной части 

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

            //Формируем экземпляр сервиса получения ссылки файла
            _getFileUrl = new GetFileUrl();

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
                _currentDetail = _details.First;

                //Строим страницу
                ChangingPart();

                //Включаем кнопки переключения детальных частей, если их больше одной
                if (_details.Count > 1)
                    GoNextButton.Visibility = Visibility.Visible;

                //Включаем кнопки переключения изображений, если их больше одной
                if (_files.Count > 1)
                    GoNextImageButton.Visibility = Visibility.Visible;
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

    /// <summary>
    /// Событие нажатия на кнопку следующего изображения
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void GoNextImageButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            //Меняем текущее изображение на следующее
            _currentFile = _currentFile.Next;

            //Меняем путь изображения
            Images.Source = new BitmapImage(new Uri(_currentFile.Value));

            //Включаем видимость кнопки преключения на предыдущее изображение
            GoBackImageButton.Visibility = Visibility.Visible;

            //Если нет следующего элемента, отключаем видимость кнопки переключения на следующее изображение
            if (_currentFile.Next == null)
                GoNextImageButton.Visibility = Visibility.Hidden;
        }
        catch (Exception ex)
        {
            SetError(ex.Message, true);
        }
    }

    /// <summary>
    /// Событие нажатия на кнопку предыдущего изображения
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void GoBackImageButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            //Меняем текущее изображение на предыдущее
            _currentFile = _currentFile.Previous;

            //Меняем путь изображения
            Images.Source = new BitmapImage(new Uri(_currentFile.Value));

            //Включаем видимость кнопки преключения на следущее изображение
            GoNextImageButton.Visibility = Visibility.Visible;

            //Если нет предыдущее элемента, отключаем видимость кнопки переключения на предыдущее изображение
            if (_currentFile.Previous == null)
                GoBackImageButton.Visibility = Visibility.Hidden;
        }
        catch (Exception ex)
        {
            SetError(ex.Message, true);
        }
    }

    /// <summary>
    /// Событие нажатия на кнопку следующей детальной части
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void GoNextButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            //Меняем текущую детальную часть на следующую
            _currentDetail = _currentDetail.Next;

            //Строим страницу
            ChangingPart();

            //Включаем видимость кнопки преключения на предыдущую детальную часть
            GoBackButton.Visibility = Visibility.Visible;

            //Если нет следующего элемента, отключаем видимость кнопки переключения на следующую детальную часть
            if (_currentDetail.Next == null)
                GoNextButton.Visibility = Visibility.Hidden;
        }
        catch (Exception ex)
        {
            SetError(ex.Message, true);
        }
    }

    /// <summary>
    /// Событие нажатия на кнопку предыдущей детальной части
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void GoBackButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            //Меняем текущую детальную часть на предыдущую
            _currentDetail = _currentDetail.Previous;

            //Строим страницу
            ChangingPart();

            //Включаем видимость кнопки преключения на следующую детальную часть
            GoNextButton.Visibility = Visibility.Visible;
            
            //Если нет предыдущую элемента, отключаем видимость кнопки переключения на предыдущую детальную часть
            if (_currentDetail.Previous == null)
                GoBackButton.Visibility = Visibility.Hidden;
        }
        catch (Exception ex)
        {
            SetError(ex.Message, true);
        }
    }

    /// <summary>
    /// Метод изменения детальной части
    /// </summary>
    private void ChangingPart()
    {
        try
        {
            //Присваиваем текст
            Text.Text = _currentDetail.Value.Text;

            //Получаем ссылки изображений
            List<string> files = new();
            foreach (var file in _currentDetail.Value.Files)
            {
                files.Add(_getFileUrl.BuilderUrl(file, _currentDetail.Value.Id ?? 0));
            }
            _files = new(files);

            //Присваиваем изображение
            _currentFile = _files.First;
            Images.Source = new BitmapImage(new Uri(_currentFile.Value));

            //Обрабатываем видимость кнопок переключения изображений
            GoBackImageButton.Visibility = Visibility.Hidden;
            GoNextImageButton.Visibility = Visibility.Visible;
        }
        catch (Exception ex)
        {
            SetError(ex.Message, true);
        }
    }
}