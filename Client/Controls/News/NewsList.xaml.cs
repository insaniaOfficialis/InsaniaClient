using Client.Controls.Bases;
using Client.Controls.Generators;
using Client.Services.Base;
using Domain.Models.Base;
using Queries.Informations.News.GetFullListNews;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Client.Controls.News;

/// <summary>
/// Логика взаимодействия для NewsList.xaml
/// </summary>
public partial class NewsList : UserControl
{
    private ILogger _logger { get { return Log.ForContext<GeneratorCreatePersonalName>(); } } //сервис для записи логов
    private IBaseService _baseService; //базовый сервис
    private LoadCircle _load = new(); //элемент загрузки
    public string _search; //строка поиска
    private long _id; //ссылка на новость
    private IGetFullListNews _getFullListNews; //сервис получения полного списка новостей
    private ObservableCollection<BaseResponseListItem> _news = new(); //список новостей

    /// <summary>
    /// Конструктор страницы получения списка новостей
    /// </summary>
    /// <param name="baseService"></param>
    /// <param name="id"></param>
    public NewsList(IBaseService baseService, long id)
    {
        try
        {
            //Инициализация всех компонентов
            InitializeComponent();

            //Присваиваем ссылку новости
            _id = id;

            //Получаем базовый сервис
            _baseService = baseService;

            //Проверяем доступность api
            _baseService.CheckConnection();

            //Формируем сервис получения полного списка новостей
            _getFullListNews = new GetFullListNews();
        }
        catch (Exception ex)
        {
            _logger.Error("InformationArticleList. Ошибка: {0}", ex);
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
            _logger.Error("InformationArticleList. SetError. Ошибка: {0}", ex);
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

            //Получаем новости
            var response = await _getFullListNews.Handler(_search);

            //Наполняем коллекцию новостей
            if (response != null && response.Items.Any())
            {
                foreach (var item in response.Items)
                    _news.Add(item);
            }
            NewsListBox.ItemsSource = _news;

            _news.First(x => x.Id == _id).IsSelected = true;
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
    /// Событие нажатия на элемент списка
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ListBoxItem_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        try
        {
            //Определяем нажатый элемент как элемент списка
            var element = sender as ListBoxItem;

            //Получаем id выбранного элемента
            long? id = Convert.ToInt64(element.Tag);

            if (id.HasValue)
            {
                //Формируем новый экземпляр информационной статьи
                /*InformationArticle informationArticle = new(id);

                //Меняем контент элемента на информационную статью
                Element.Content = informationArticle;*/
            }
            else
                SetError("Не удалось определить нажатый элемент", false);
        }
        catch (Exception ex)
        {
            SetError(ex.Message, true);
        }
    }
}
