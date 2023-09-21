using Client.Controls.Administrators;
using Client.Models.Base;
using Client.Models.Geography.Countries.Response;
using Microsoft.AspNetCore.WebUtilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace Client.Controls.Statistics;

/// <summary>
/// Логика взаимодействия для Countries.xaml
/// </summary>
public partial class Countries : UserControl
{
    public ILogger _logger { get { return Log.ForContext<Roles>(); } } //сервис для записи логов
    private readonly JsonSerializerOptions _settings = new(); //настройки десериализации json
    private int? skip, take = 20;
    private List<BaseSortRequest?>? sort = new();
    private string? search;
    ObservableCollection<CountriesResponseListItem?>? countries = new();

    /// <summary>
    /// Конструктор страницы стран
    /// </summary>
    public Countries()
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
            _logger.Error("Countries. Ошибка: {1}", ex);
        }
    }

    /// <summary>
    /// Метод проверки соединения с api
    /// </summary>
    public async void CheckConnection()
    {
        try
        {
            //Блокируем все элементы
            CountriesDataGrid.IsEnabled = false;

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
                        //Разблокируем все элементы
                        CountriesDataGrid.IsEnabled = true;

                        //Вызываем метод получения стран
                        GetCountries();
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
            _logger.Error("Countries. SetError. Ошибка: {1}", ex);
        }
    }

    /// <summary>
    /// Метод получения стран
    /// </summary>
    public async void GetCountries()
    {
        try
        {
            //Объявляем переменную ссылки запроса
            string url = null;

            //Если в конфиге есть данные для формирования ссылки запроса
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["DefaultConnection"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Api"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Countries"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Token"]))
            {
                //Формируем ссылку запроса
                url = ConfigurationManager.AppSettings["DefaultConnection"] + ConfigurationManager.AppSettings["Api"] +
                    ConfigurationManager.AppSettings["Countries"] + "listFull";

                //Устанавливаем первоначальные параметры пагинации и сортировки
                skip = 0;
                sort.Add(new("number", true));

                //Добавляем параметры строки
                url += CreateQueryStringListCountries();

                //Формируем клиента и добавляем токен
                using HttpClient client = new();

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ConfigurationManager.AppSettings["Token"]);

                //Получаем данные по запросу
                using var result = await client.GetAsync(url);

                //Если получили успешный результат
                if (result != null && result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    //Десериализуем ответ и заполняем combobox ролей
                    var content = await result.Content.ReadAsStringAsync();

                    foreach(var item in JsonSerializer.Deserialize<CountriesResponseList>(content, _settings).Items)
                    {
                        countries.Add(item);
                    }

                    CountriesDataGrid.ItemsSource = countries;
                }
                else
                {
                    if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        SetError("Некорректный токен", false);
                    else
                        SetError("Ошибка сервера", true);
                }
            }
            else
                SetError("Не указаны адреса api. Обратитесь в техническую поддержку", true);
        }
        catch (Exception ex)
        {
            SetError(ex.Message, true);
        }
    }

    /// <summary>
    /// Формирование строки запроса для списка стран
    /// </summary>
    /// <returns></returns>
    public string CreateQueryStringListCountries()
    {
        //Формируем ссылку
        string url = "?search=";

        //Если есть строка поиска добавляем в ссылку
        if (!String.IsNullOrEmpty(search))
            url += search;

        //Если указано количество пропущенных элементов, добавляем
        if(skip != null)
            url += string.Format("&skip={0}", skip);

        //Если указано количество формируемых элементов, добавляем
        if (take != null)
            url += string.Format("&take={0}", take);

        //Если есть поля сортировки
        if(sort.Any())
        {
            //Проходим по всем полям сортировки
            for(int i = 0; i < sort.Count; i++)
            {
                //Добавляем ключ сортировки
                url += string.Format("&sort[{0}].SortKey={1}", i, sort[i].SortKey);
                //Добавляем порядок сортировки
                url += string.Format("&sort[{0}].IsAscending={1}", i, sort[i].IsAscending);
            }
        }

        //Возвращаем результат
        return url;
    }

    /// <summary>
    /// Метод пагинации на странице
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void PaginationButton_Click(object sender, RoutedEventArgs e)

    {
        try
        {
            //Объявляем переменную ссылки запроса
            string url = null;

            //Если в конфиге есть данные для формирования ссылки запроса
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["DefaultConnection"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Api"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Countries"])
                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Token"]))
            {
                //Формируем ссылку запроса
                url = ConfigurationManager.AppSettings["DefaultConnection"] + ConfigurationManager.AppSettings["Api"] +
                    ConfigurationManager.AppSettings["Countries"] + "listFull";

                //Устанавливаем первоначальные параметры пагинации и сортировки
                skip += take;

                //Добавляем параметры строки
                url += CreateQueryStringListCountries();

                //Формируем клиента и добавляем токен
                using HttpClient client = new();

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ConfigurationManager.AppSettings["Token"]);

                //Получаем данные по запросу
                using var result = await client.GetAsync(url);

                //Если получили успешный результат
                if (result != null && result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    //Десериализуем ответ и заполняем combobox ролей
                    var content = await result.Content.ReadAsStringAsync();

                    var respose = JsonSerializer.Deserialize<CountriesResponseList>(content, _settings).Items;

                    foreach (var item in respose)
                    {
                        countries.Add(item);
                    }

                    if (respose.Count < take)
                        PaginationButton.Visibility = Visibility.Hidden;

                    CountriesDataGrid.Items.Refresh();
                }
                else
                {
                    if (result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        SetError("Некорректный токен", false);
                    else
                        SetError("Ошибка сервера", true);
                }
            }
            else
                SetError("Не указаны адреса api. Обратитесь в техническую поддержку", true);
        }
        catch (Exception ex)
        {
            SetError(ex.Message, true);
        }
    }
}
