using Client.Models.Base;
using Serilog;
using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace Client.Controls.Generators
{
    /// <summary>
    /// Логика взаимодействия для GeneratorPersonalNames.xaml
    /// </summary>
    public partial class GeneratorPersonalNames : UserControl
    {
        private ILogger _logger { get { return Log.ForContext<GeneratorPersonalNames>(); } } //сервис для записи логов
        private readonly JsonSerializerOptions _settings = new(); //настройки десериализации json

        /// <summary>
        /// Конструктор страницы генерации личных имён
        /// </summary>
        public GeneratorPersonalNames()
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
                _logger.Error("GeneratorPersonalNames. Ошибка: {0}", ex);
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
                            //Вызываем метод получения рас
                            GetRaces();
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
                _logger.Error("GeneratorPersonalNames. SetError. Ошибка: {0}", ex);
            }
        }

        /// <summary>
        /// Метод получения рас
        /// </summary>
        public async void GetRaces()
        {
            try
            {
                //Объявляем переменную ссылки запроса
                string path = null;

                //Если в конфиге есть данные для формирования ссылки запроса
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["DefaultConnection"])
                    && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Api"])
                    && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Races"])
                    && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["Token"]))
                {
                    //Формируем ссылку запроса
                    path = ConfigurationManager.AppSettings["DefaultConnection"] + ConfigurationManager.AppSettings["Api"] +
                        ConfigurationManager.AppSettings["Races"] + "list";

                    //Формируем клиента и добавляем токен
                    using HttpClient client = new();

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ConfigurationManager.AppSettings["Token"]);

                    //Получаем данные по запросу
                    using var result = await client.GetAsync(path);

                    //Если получили успешный результат
                    if (result != null && result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        //Десериализуем ответ и заполняем combobox ролей
                        var content = await result.Content.ReadAsStringAsync();

                        BaseResponseList response = JsonSerializer.Deserialize<BaseResponseList>(content, _settings);

                        RacesComboBox.ItemsSource = response.Items;
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
        /// Событие выбора в выпадающем списке рас
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RacesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                //Включаем выпадающий список наций
                NationsComboBox.IsEnabled = true;
            }
            catch (Exception ex)
            {
                _logger.Error("GeneratorPersonalNames. RacesComboBox_Selected. Ошибка: {0}", ex);
            }
        }

        /// <summary>
        /// Событие выбора в выпадающем списке наций
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NationsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                //Включаем выпадающий список наций
                GenerateButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                _logger.Error("GeneratorPersonalNames. RacesComboBox_Selected. Ошибка: {0}", ex);
            }

        }
    }
}
