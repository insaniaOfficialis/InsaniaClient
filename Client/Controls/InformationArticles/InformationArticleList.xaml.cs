using Client.Controls.Bases;
using Client.Controls.Generators;
using Client.Services.Base;
using Domain.Models.Base;
using Queries.Informations.InformationArticles.GetListInformationArticles;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Client.Controls.InformationArticles;

/// <summary>
/// Логика взаимодействия для InformationArticle.xaml
/// </summary>
public partial class InformationArticleList : UserControl
{
    private ILogger _logger { get { return Log.ForContext<GeneratorCreatePersonalName>(); } } //сервис для записи логов
    private IBaseService _baseService; //базовый сервис
    private IGetListInformationArticles _getListInformationArticles; //сервис получения списка информационных статей
    private LoadCircle _load = new(); //элемент загрузки
    private ObservableCollection<BaseResponseListItem> _informationArticles = new(); //коллекция информационных статей
    public string _search; //строка поиска

    /// <summary>
    /// Конструктор страницы списка информациионных статей
    /// </summary>
    /// <param name="baseService"></param>
    public InformationArticleList(IBaseService baseService)
    {
        try
        {
            //Инициализация всех компонентов
            InitializeComponent();

            //Получаем базовый сервис
            _baseService = baseService;

            //Проверяем доступность api
            _baseService.CheckConnection();

            //Формируем сервис получения логов
            _getListInformationArticles = new GetListInformationArticles();
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

            //Получаем информационные статьи
            var response = await _getListInformationArticles.Handler(_search);

            //Наполняем коллекцию логов
            if (response != null && response.Items.Any())
            {
                foreach (var item in response.Items)
                    _informationArticles.Add(item);
            }

            InformationArticlesListBox.ItemsSource = _informationArticles;
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
                InformationArticle informationArticle = new(id);

                //Меняем контент элемента на информационную статью
                Element.Content = informationArticle;
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