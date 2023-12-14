using Client.Controls.Bases;
using Client.Services.Base;
using Domain.Models.Base;
using Domain.Models.Identification.Users.Internal;
using Domain.Models.Informations.News.Response;
using Domain.Models.Informations.NewsDetails.Response;
using Queries.Informations.News.AddNews;
using Queries.Informations.News.EditNews;
using Queries.Informations.News.GetNewsDetailsFull;
using Queries.Informations.News.GetNewsTypes;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client.Controls.Administrators.News;

/// <summary>
/// Логика взаимодействия для SingleNewsManagment.xaml
/// </summary>
public partial class SingleNewsManagment : Window
{
    long? _id; //id записи
    public ILogger _logger { get { return Log.ForContext<SingleNewsManagment>(); } } //логгер для записи логов
    readonly JsonSerializerOptions _settings = new(); //настройки десериализации json
    public IBaseService _baseService; //базовый сервис
    private LoadCircle _load = new(); //элемент загрузки
    private IGetNewsTypes _getNewsTypes; //получение типов новостей
    private IGetNewsDetailsFull _getNewsDetailsFull; //получение детальных частей новости
    private IAddNews _addNews; //добавление новости
    private IEditNews _editNews; //редактирование новости
    private ObservableCollection<BaseResponseListItem> _newsTypes = new(); //коллекция типов новостей
    private ObservableCollection<GetNewsDetailsFullResponseItem> _newsDetails = new(); //коллекция детальных частей новости
    private AccessRightAction _accessRightAction = new(); //права доступа
    private long? _typeId; //тип новости

    /// <summary>
    /// Создание новости
    /// </summary>
    /// <param name="baseService"></param>
    /// <param name="accessRights"></param>
    public SingleNewsManagment(IBaseService baseService, List<string> accessRights)
    {
        try
        {
            //Инициализируем компоненты
            InitializeComponent();

            //Выставляем параметры десериализации
            _settings.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

            //Получаем базовый сервис
            _baseService = baseService;

            //Проверяем доступность api
            _baseService.CheckConnection();

            //Формируем экземпляры сервисов
            _getNewsTypes = new GetNewsTypes();
            _getNewsDetailsFull = new GetNewsDetailsFull();
            _addNews = new AddNews();
            _editNews = new EditNews();

            //Если есть право доступа "Добавление новости"
            if (accessRights.Contains("Dobavlenie_detal'noy_chasti_novosti"))
                AddButton.Visibility = Visibility.Visible;

            //Если есть право доступа "Редактирование новости"
            if (accessRights.Contains("Redaktirovanie_detal'noy_chasti_novosti"))
                _accessRightAction.Edit = true;

            //Если есть право доступа "Удаление новости"
            if (accessRights.Contains("Udalenie_detal'noy_chasti_novosti"))
                _accessRightAction.Delete = true;

            //Если есть право доступа "Восстановление новости"
            if (accessRights.Contains("Vosstanovlenie_detal'noy_chasti_novosti"))
                _accessRightAction.Restore = true;
        }
        catch (Exception ex)
        {
            _logger.Error("SingleNewsManagment. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Редактирование новости
    /// </summary>
    /// <param name="baseService"></param>
    /// <param name="accessRights"></param>
    /// <param name="id"></param>
    /// <param name="title"></param>
    /// <param name="introduction"></param>
    /// <param name="ordinalNumber"></param>
    /// <param name="typeId"></param>
    public SingleNewsManagment(IBaseService baseService, List<string> accessRights, long id, string title, string introduction,
        long ordinalNumber, long typeId) : this(baseService, accessRights)
    {
        try
        {
            //Заполняем поля ввода
            TitleTextBox.Text = title;
            IntroductionTextBox.Text = introduction;
            OrdinalNumberTextBox.Text = ordinalNumber.ToString();
            
            //Заполняем тип
            _typeId = typeId;

            //Заполняем id
            _id = id;
        }
        catch (Exception ex)
        {
            _logger.Error("SingleNewsManagment. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Метод обнуления полей ввода по нажатию
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        try
        {
            var textbox = sender as TextBox;

            switch (textbox.Name)
            {
                case "TitleTextBox":
                    {
                        if (TitleTextBox.Text == "Заголовок")
                            TitleTextBox.Text = "";
                    }
                    break;
                case "IntroductionTextBox":
                    {
                        if (IntroductionTextBox.Text == "Вступление")
                            IntroductionTextBox.Text = "";
                    }
                    break;
                case "OrdinalNumberTextBox":
                    {
                        if (OrdinalNumberTextBox.Text == "Порядковый номер")
                            OrdinalNumberTextBox.Text = "";
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.Error("SingleNewsManagment. TextBox_GotFocus. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Метод возвращения значений по умолчанию полей ввода по потере фокуса
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        try
        {
            var textbox = sender as TextBox;

            switch (textbox.Name)
            {
                case "TitleTextBox":
                    {
                        if (TitleTextBox.Text == "")
                            TitleTextBox.Text = "Заголовок";
                    }
                    break;
                case "IntroductionTextBox":
                    {
                        if (IntroductionTextBox.Text == "")
                            IntroductionTextBox.Text = "Вступление";
                    }
                    break;
                case "OrdinalNumberTextBox":
                    {
                        if (OrdinalNumberTextBox.Text == "")
                            OrdinalNumberTextBox.Text = "Порядковый номер";
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.Error("SingleNewsManagment. TextBox_LostFocus. Ошибка: {0}", ex);
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
            var serverExceptionStyle = FindResource(style) as Style;

            //Устанавливаем текст и стиль
            ErrorText.Style = serverExceptionStyle;
            ErrorText.Text = message;

            //Включаем кнопку сохранения
            SaveButton.IsEnabled = true;
        }
        catch (Exception ex)
        {
            _logger.Error("SingleNewsManagment. SetError. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Метод сохранения
    /// </summary>
    private async void Save()
    {
        try
        {
            //Включаем элемент загрузки
            LoadContent.Content = _load;
            LoadContent.Visibility = Visibility.Visible;

            //Если нет первичного ключа
            if(_id == null)
                //Отправляем запрос на добавление новости
                await _addNews.Handler(TitleTextBox.Text, IntroductionTextBox.Text, TypeCombobox.SelectedValue as long?
                    ,string.IsNullOrEmpty(OrdinalNumberTextBox.Text) ? null : Convert.ToInt64(OrdinalNumberTextBox.Text));
            //Иначе
            else
                //Отправляем запрос на редактирование новости
                await _editNews.Handler(TitleTextBox.Text, IntroductionTextBox.Text, TypeCombobox.SelectedValue as long?
                    ,string.IsNullOrEmpty(OrdinalNumberTextBox.Text) ? null : Convert.ToInt64(OrdinalNumberTextBox.Text), _id);

            //Закрываем окно и выводим сообщение об успешности
            Close();
            Message message = new("Успешно");
            message.Show();

        }
        catch (Exception ex)
        {
            SetError("Не удалось сохранить. Обратитесь в техническую поддержку", true);
            _logger.Error("SingleNewsManagment. Save. Ошибка: {0}", ex);
        }
        finally
        {
            //Отключаем элемент загрузки
            LoadContent.Content = null;
            LoadContent.Visibility = Visibility.Visible;
        }
    }

    /// <summary>
    /// Событие нажатия клавиши сохранить
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            //Вызываем сохранение
            Save();
        }
        catch (Exception ex)
        {
            _logger.Error("SingleNewsManagment. SaveButton_Click. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Событие нажатия клавиш
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        try
        {
            //Если нажата клавиша eacape
            if (e.Key == Key.Escape)
                //Закрываем окно
                Close();

            //Если нажата клавиша enter
            if (e.Key == Key.Enter)
                //Вызываем сохранение
                Save();
        }
        catch (Exception ex)
        {
            _logger.Error("SingleNewsManagment. Window_PreviewKeyDown. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Событие перетягивания мыши
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            //Если левая кнопка мыши
            if (e.ChangedButton == MouseButton.Left)
                //Включаем перетягивание
                DragMove();
        }
        catch (Exception ex)
        {
            _logger.Error("SingleNewsManagment. Window_MouseDown. Ошибка: {0}", ex);
        }
    }

    /// <summary>
    /// Событие загрузки окна
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            //Включаем элемент загрузки
            LoadContent.Content = _load;
            LoadContent.Visibility = Visibility.Visible;

            var newsTypes = GetNewsTypes();

            //Если нет первичного ключа
            if (_id != null)
            {
                var newsDetails = GetNewsDetails();

                await Task.WhenAll(newsTypes, newsDetails);
            }
            else
                await newsTypes;
        }
        catch (Exception ex)
        {
            SetError(ex.Message, true);
        }
        finally
        {
            //Отключаем элемент загрузки
            LoadContent.Content = null;
            LoadContent.Visibility = Visibility.Visible;
        }
    }

    /// <summary>
    /// Получение типов новостей
    /// </summary>
    /// <returns></returns>
    private async Task GetNewsTypes()
    {
        //Получаем типы новостей
        var response = await _getNewsTypes.Handler();

        //Наполняем коллекцию типов новостей
        if (response != null && response.Items.Any())
        {
            foreach (var item in response.Items)
            {
                _newsTypes.Add(item);
            }
        }

        //Привязываем источник данных для выпадающего списка
        TypeCombobox.ItemsSource = _newsTypes;

        //Записываем тип, если он указан
        if(_typeId != null)
            TypeCombobox.SelectedValue = _typeId;
    }

    /// <summary>
    /// Получение детальных частей новости
    /// </summary>
    /// <returns></returns>
    private async Task GetNewsDetails()
    {
        //Получаем детальныe части новости
        var response = await _getNewsDetailsFull.Handler(_id);

        //Наполняем коллекцию детальных частей новости
        if (response != null && response.Items.Count != 0)
        {
            foreach (var item in response.Items)
            {
                if (_accessRightAction != null)
                {
                    item.Edit = _accessRightAction.Edit;
                    item.Delete = _accessRightAction.Delete;
                    item.Restore = _accessRightAction.Restore;
                }
                _newsDetails.Add(item);
            }
        }

        //Привязываем источник данных для таблицы
        NewsDetailsDataGrid.ItemsSource = _newsDetails;
    }

    /// <summary>
    /// Событие нажатия на кнопку добавления детальной части новости
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void AddButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            //Формируем экземпляр окно управления детальной части новости
            NewsDetailManagment newsDetailManagment = new(_baseService);

            //Отображаем окно
            newsDetailManagment.ShowDialog();

            //Обновляем детальные части новости
            _newsDetails.Clear();
            await GetNewsDetails();
        }
        catch (Exception ex)
        {
            SetError(ex.Message, true);
        }
    }

    /// <summary>
    /// Событие нажатия на кнопку редактирования детальной части новости
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void EditButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            //Получаем данные из выбранной строки
            GetNewsDetailsFullResponseItem item = NewsDetailsDataGrid.SelectedItem
                as GetNewsDetailsFullResponseItem;

            //Формируем экземпляр окно управления детальной части новости
            NewsDetailManagment newsDetailManagment = new(_baseService, item.Id ?? 0,
                item.Text, item.OrdinalNumber ?? 0);

            //Отображаем окно
            newsDetailManagment.ShowDialog();

            //Обновляем детальные части новости
            _newsDetails.Clear();
            await GetNewsDetails();
        }
        catch(Exception ex)
        {
            SetError(ex.Message, true);
        }
    }

    private void DeletedButton_Click(object sender, RoutedEventArgs e)
    {

    }

    private void RestoredButton_Click(object sender, RoutedEventArgs e)
    {

    }
}